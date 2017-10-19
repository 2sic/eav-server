using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;
using Entity = ToSic.Eav.Data.Entity;

namespace ToSic.Eav.Apps.ImportExport
{

    // todo:
    // if possible, split appart into
    // 1. xml > entity
    // 2. entity > db
    // core dependencies are the data-structure of the content-type, which is used to build the import-entity
    // it also looks up data in the DB to validate if they already exist - to see if it's a new/update scenario

    /// <summary>
    /// Import a virtual table of content-items
    /// </summary>
    public partial class ImportListXmlRefactoring: HasLog, ToSic.Eav.Apps.ImportExport.removing.IImportListTemp
    {
        private IContentType ContentType { get; }

        private AppDataPackage App { get; }

        /// <summary>
        /// Create a xml import. The data stream passed will be imported to memory, and checked 
        /// for errors. If no error could be found, the data can be persisted to the repository.
        /// </summary>
        /// <param name="appPackage"></param>
        /// <param name="contentType">content-type</param>
        /// <param name="dataStream">Xml data stream to import</param>
        /// <param name="languages">Languages that can be imported (2SexyContent languages enabled)</param>
        /// <param name="documentLanguageFallback">Fallback document language</param>
        /// <param name="deleteSetting">How to handle entities already in the repository</param>
        /// <param name="resolveReferenceMode">How value references to files and pages are handled</param>
        /// <param name="parentLog"></param>
        public ImportListXmlRefactoring(AppDataPackage appPackage, IContentType contentType, Stream dataStream, IEnumerable<string> languages, string documentLanguageFallback, 
            ImportDeleteUnmentionedItems deleteSetting, 
            ImportResourceReferenceMode resolveReferenceMode, 
            Log parentLog): base("App.ImpVT", parentLog, "building xml vtable import")
        {
            ImportEntities = new List<Entity>();
            ErrorLog = new ImportErrorLog();

            App = appPackage;

            _appId = App.AppId;
            //_zoneId = zoneId;

            ContentType = contentType;// DbContext.AttribSet.GetDbAttribSet(contentTypeId);
            if (ContentType == null)
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidContentType);
                return;
            }

            AttributesOfType = contentType.Attributes;// DbContext.AttributesDefinition.GetAttributeDefinitions(contentTypeId).ToList();
            ExistingEntities = App.Entities.Values.Where(e => e.Type == contentType).ToList();// DbContext.Entities.GetEntitiesByType(ContentType).ToList();

            _languages = languages;
            if (_languages == null || !_languages.Any())
                _languages = new[] { string.Empty };

            _documentLanguageFallback = documentLanguageFallback;
            _deleteSetting = deleteSetting;
            _resolveReferenceMode = resolveReferenceMode;

            Timer.Start();
            try
            {
                if (!LoadStreamIntoDocumentElement(dataStream)) return;
                if (!RunDocumentValidityChecks()) return;
                ValidateAndImportToMemory();
            }
            catch (Exception exception)
            {
                ErrorLog.AppendError(ImportErrorCode.Unknown, exception.ToString());
            }
            Timer.Stop();
            TimeForMemorySetup = Timer.ElapsedMilliseconds;
        }
        private IList<IAttributeDefinition> AttributesOfType { get; }
        private List<IEntity> ExistingEntities { get; }

        private IEntity FindInExisting(Guid guid)
            => ExistingEntities.FirstOrDefault(e => e.EntityGuid == guid);

        /// <summary>
        /// Deserialize data xml stream to the memory. The data will also be checked for 
        /// errors.
        /// </summary>
        private void ValidateAndImportToMemory()
        {
            var documentElementNumber = 0;
            var entityGuidManager = new ImportItemGuidManager();

            foreach (var documentElement in DocumentElements)
            {
                documentElementNumber++;

                var documentElementLanguage = documentElement.Element(XmlConstants.EntityLanguage)?.Value;
                if (_languages.All(language => language != documentElementLanguage))
                {
                    // DNN does not support the language
                    ErrorLog.AppendError(ImportErrorCode.InvalidLanguage, "Lang=" + documentElementLanguage,
                        documentElementNumber);
                    continue;
                }

                var entityGuid = entityGuidManager.GetGuid(documentElement, _documentLanguageFallback);
                var entity = GetImportEntity(entityGuid) ?? AppendEntity(entityGuid);

                foreach (var attribute in AttributesOfType)
                {
                    var valueType = attribute.Type;
                    var valueName = attribute.Name;
                    var value = documentElement.Element(valueName)?.Value;
                    if (value == null || value == XmlConstants.Null)
                        continue;

                    if (value == XmlConstants.Empty)
                    {
                        // It is an empty string
                        entity.Attributes.AddValue(valueName, "", attribute.Type, documentElementLanguage, false,
                            _resolveReferenceMode == ImportResourceReferenceMode.Resolve);
                        continue;
                    }

                    var valueReferenceLanguage = value.GetLanguageInARefTextCode()?.ToLowerInvariant();
                    if (valueReferenceLanguage == null) // It is not a value reference.. it is a normal text
                    {
                        try
                        {
                            entity.Attributes.AddValue(valueName, value, valueType, documentElementLanguage, false,
                                _resolveReferenceMode == ImportResourceReferenceMode.Resolve);
                        }
                        catch (FormatException)
                        {
                            ErrorLog.AppendError(ImportErrorCode.InvalidValueFormat,
                                valueName + ":" + valueType + "=" + value, documentElementNumber);
                        }
                        continue;
                    }

                    var valueReferenceProtection = value.GetValueReferenceProtection();
                    if (valueReferenceProtection != XmlConstants.ReadWrite && valueReferenceProtection != XmlConstants.ReadOnly)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReferenceProtection, value,
                            documentElementNumber);
                        continue;
                    }
                    var valueReadOnly = valueReferenceProtection == XmlConstants.ReadOnly;

                    // if this value is just a placeholder/reference to another value,
                    // then find the master record, and add this language to it's users
                    var entityValue = entity.Attributes.FindItemOfLanguage(valueName, valueReferenceLanguage);
                    if (entityValue != null)
                    {
                        entityValue.Languages.Add(new Dimension { Key = documentElementLanguage, ReadOnly = valueReadOnly });
                        continue;
                    }

                    // We do not have the value referenced in memory, so search for the 
                    // value in the cache 
                    var existingEnt = FindInExisting(entityGuid);
                    if (existingEnt == null)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReference, value, documentElementNumber);
                        continue;
                    }

                    //var valExisting = existingEnt[attribute.Name].GetValueOfExactLanguage(attribute, valueReferenceLanguage);
                    var valExisting =
                        ExportListXml.GetExactAssignedValue(existingEnt[attribute.Name], valueReferenceLanguage, null);
                    if (valExisting == null)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReference, value, documentElementNumber);
                        continue;
                    }

                    entity.Attributes.AddValue(valueName, valExisting, valueType, valueReferenceLanguage,
                            //valExisting.IsLanguageReadOnly(valueReferenceLanguage),
                            valExisting.Languages.FirstOrDefault(l => l.Key == valueReferenceLanguage)?.ReadOnly ?? false,
                            _resolveReferenceMode == ImportResourceReferenceMode.Resolve)
                        //.AddLanguageReference(documentElementLanguage, valueReadOnly);
                        .Languages.Add(new Dimension { Key = documentElementLanguage, ReadOnly = valueReadOnly });
                }
            }

        }


        /// <summary>
        /// Save the data in memory to the repository.
        /// </summary>
        /// <param name="userId">ID of the user doing the import</param>
        /// <returns>True if succeeded</returns>
        public bool PersistImportToRepository(string userId)
        {
            if (ErrorLog.HasErrors)
                return false;

            DbDataController dbContext = DbDataController.Instance(null, App.AppId, Log);

            if (_deleteSetting == ImportDeleteUnmentionedItems.All)
            {
                var entityDeleteGuids = GetEntityDeleteGuids();
                foreach(var entityGuid in entityDeleteGuids)
                {
                    var entityId = FindInExisting(entityGuid).EntityId;
                    if (dbContext.Entities.CanDeleteEntity(entityId).Item1)
                        dbContext.Entities.DeleteEntity(entityId);
                }
            }

            Timer.Start();
            var import = new Import(/*_zoneId*/ null, _appId, false);
            import.ImportIntoDb(null, ImportEntities);
            // important note: don't purge cache here, but the caller MUST do this!

            Timer.Stop();
            TimeForDbImport = Timer.ElapsedMilliseconds;
            return true;
        }


        #region Deserialize statistics methods
        private List<Guid> GetExistingEntityGuids()
        {
            var existingGuids = ExistingEntities 
                .Select(entity => entity.EntityGuid).ToList();
            return existingGuids;
        }


        /// <summary>
        /// Get the attribute names in the content type.
        /// </summary>
        public IEnumerable<string> AttributeNamesInContentType 
            => AttributesOfType.Select(item => item.Name).ToList();

        #endregion Deserialize statistics methods
        
    }




    internal static class StringExtension
    {
        /// <summary>
        /// Get for example en-US from [ref(en-US,ro)].
        /// </summary>
        public static string GetLanguageInARefTextCode(this string valueString)
        {
            var match = Regex.Match(valueString, @"\[ref\((?<language>.+),(?<readOnly>.+)\)\]");
            return match.Success ? match.Groups["language"].Value : null;
        }

        /// <summary>
        /// Get for example ro from [ref(en-US,ro)].
        /// </summary>
        public static string GetValueReferenceProtection(this string valueString, string defaultValue = "")
        {
            var match = Regex.Match(valueString, @"\[ref\((?<language>.+),(?<readOnly>.+)\)\]");
            return match.Success ? match.Groups["readOnly"].Value : defaultValue;
        }

    }
}
