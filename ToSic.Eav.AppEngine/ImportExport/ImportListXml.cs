using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Entity = ToSic.Eav.Data.Entity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import a virtual table of content-items
    /// </summary>
    public partial class ImportListXml: HasLog 
    {
        private IContentType ContentType { get; }
        private List<IEntity> ExistingEntities { get; }

        private AppDataPackage App { get; }
        private AppManager AppMan { get; }

        /// <summary>
        /// Create a xml import. The data stream passed will be imported to memory, and checked 
        /// for errors. If no error could be found, the data can be persisted to the repository.
        /// </summary>
        /// <param name="appMan"></param>
        /// <param name="contentType">content-type</param>
        /// <param name="dataStream">Xml data stream to import</param>
        /// <param name="languages">Languages that can be imported (2SexyContent languages enabled)</param>
        /// <param name="documentLanguageFallback">Fallback document language</param>
        /// <param name="deleteSetting">How to handle entities already in the repository</param>
        /// <param name="resolveReferenceMode">How value references to files and pages are handled</param>
        /// <param name="parentLog"></param>
        public ImportListXml(AppManager appMan, IContentType contentType, Stream dataStream, IEnumerable<string> languages, string documentLanguageFallback, 
            ImportDeleteUnmentionedItems deleteSetting, 
            ImportResourceReferenceMode resolveReferenceMode, 
            Log parentLog): base("App.ImpVT", parentLog, "building xml vtable import")
        {
            ImportEntities = new List<Entity>();
            ErrorLog = new ImportErrorLog();

            AppMan = appMan;
            App = appMan.Cache.AppDataPackage;

            _appId = App.AppId;

            ContentType = contentType;
            if (ContentType == null)
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidContentType);
                return;
            }

            ExistingEntities = App.Entities.Values.Where(e => e.Type == contentType).ToList();

            _languages = languages;
            if (_languages == null || !_languages.Any())
                _languages = new[] { string.Empty };

            _languages = _languages.Select(l => l.ToLowerInvariant());

            _documentLanguageFallback = documentLanguageFallback.ToLowerInvariant();
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

        /// <summary>
        /// Deserialize data xml stream to the memory. The data will also be checked for 
        /// errors.
        /// </summary>
        private void ValidateAndImportToMemory()
        {
            var nodesCount = 0;
            var entityGuidManager = new ImportItemGuidManager();

            foreach (var documentElement in DocumentElements)
            {
                nodesCount++;

                var nodeLang = documentElement.Element(XmlConstants.EntityLanguage)?.Value.ToLowerInvariant();
                if (_languages.All(language => language != nodeLang))
                {
                    // problem when DNN does not support the language
                    ErrorLog.AppendError(ImportErrorCode.InvalidLanguage, "Lang=" + nodeLang,
                        nodesCount);
                    continue;
                }

                var entityGuid = entityGuidManager.GetGuid(documentElement, _documentLanguageFallback);
                var entity = GetImportEntity(entityGuid) ?? AppendEntity(entityGuid);

                foreach (var attribute in ContentType.Attributes)
                {
                    var valueType = attribute.Type;
                    var valueName = attribute.Name;
                    var value = documentElement.Element(valueName)?.Value;
                    if (value == null || value == XmlConstants.Null)
                        continue;

                    if (value == XmlConstants.Empty)
                    {
                        // It is an empty string
                        entity.Attributes.AddValue(valueName, "", attribute.Type, nodeLang, false,
                            _resolveReferenceMode == ImportResourceReferenceMode.Resolve);
                        continue;
                    }

                    var valueReferenceLanguage = value.GetLanguageInARefTextCode()?.ToLowerInvariant();
                    if (valueReferenceLanguage == null) // It is not a value reference.. it is a normal text
                    {
                        try
                        {
                            entity.Attributes.AddValue(valueName, value, valueType, nodeLang, false,
                                _resolveReferenceMode == ImportResourceReferenceMode.Resolve);
                        }
                        catch (FormatException)
                        {
                            ErrorLog.AppendError(ImportErrorCode.InvalidValueFormat,
                                valueName + ":" + valueType + "=" + value, nodesCount);
                        }
                        continue;
                    }

                    var valueReferenceProtection = value.GetValueReferenceProtection();
                    if (valueReferenceProtection != XmlConstants.ReadWrite && valueReferenceProtection != XmlConstants.ReadOnly)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReferenceProtection, value,
                            nodesCount);
                        continue;
                    }
                    var valueReadOnly = valueReferenceProtection == XmlConstants.ReadOnly;

                    // if this value is just a placeholder/reference to another value,
                    // then find the master record, and add this language to it's users
                    var entityValue = entity.Attributes.FindItemOfLanguage(valueName, valueReferenceLanguage);
                    if (entityValue != null)
                    {
                        entityValue.Languages.Add(new Dimension { Key = nodeLang, ReadOnly = valueReadOnly });
                        continue;
                    }

                    // so search for the value in the cache 
                    var existingEnt = FindInExisting(entityGuid);
                    if (existingEnt == null)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReference, value, nodesCount);
                        continue;
                    }

                    //var valExisting = existingEnt[attribute.Name].GetValueOfExactLanguage(attribute, valueReferenceLanguage);
                    var valExisting =
                        ExportListXml.GetExactAssignedValue(existingEnt[attribute.Name], valueReferenceLanguage, null);
                    if (valExisting == null)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReference, value, nodesCount);
                        continue;
                    }

                    entity.Attributes.AddValue(valueName, valExisting, valueType, valueReferenceLanguage,
                            //valExisting.IsLanguageReadOnly(valueReferenceLanguage),
                            valExisting.Languages.FirstOrDefault(l => l.Key == valueReferenceLanguage)?.ReadOnly ?? false,
                            _resolveReferenceMode == ImportResourceReferenceMode.Resolve)
                        //.AddLanguageReference(documentElementLanguage, valueReadOnly);
                        .Languages.Add(new Dimension { Key = nodeLang, ReadOnly = valueReadOnly });
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

            Timer.Start();
            if (_deleteSetting == ImportDeleteUnmentionedItems.All)
                AppMan.Entities.Delete(GetEntityDeleteGuids()
                    .Select(g => FindInExisting(g).EntityId).ToList());

            var import = new Import(null, _appId, false);
            import.ImportIntoDb(null, ImportEntities);
            // important note: don't purge cache here, but the caller MUST do this!

            Timer.Stop();
            TimeForDbImport = Timer.ElapsedMilliseconds;
            return true;
        }
        
    }




}
