using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;
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
    public class ToRefactorXmlImportVTable
    {

        #region properties like _appId, Document, etc.

        #region Timing / Debuging infos

        /// <summary>
        /// Helper to measure time used for stuff
        /// </summary>
        public Stopwatch Timer { get; set; } = new Stopwatch();

        public long TimeForMemorySetup;
        public long TimeForDbImport;
        #endregion

        private Log Log { get; }

        private readonly int _appId;
        private readonly int _zoneId;

        private ToSicEavAttributeSets ContentType { get; }

        /// <summary>
        /// The xml document to imported.
        /// </summary>
        public XDocument Document  { get; private set; }

        /// <summary>
        /// The elements of the xml document.
        /// </summary>
        public IEnumerable<XElement> DocumentElements { get; private set; }

        private readonly string _documentLanguageFallback;

        private IEnumerable<string> _languages;

        private readonly ImportResourceReferenceMode _resolveReferenceMode;

        private readonly ImportDeleteUnmentionedItems _deleteSetting;

        /// <summary>
        /// The entities created from the document. They will be saved to the repository.
        /// </summary>
        public List<Entity> ImportEntities {get; }
        private Entity GetImportEntity(Guid entityGuid) => ImportEntities
            .FirstOrDefault(entity => entity.EntityGuid == entityGuid);

        /// <summary>
        /// Errors found while importing the document to memory.
        /// </summary>
        public ImportErrorLog ErrorLog { get; }
        #endregion


        private Entity AppendEntity(Guid entityGuid)
        {
            var entity = new Entity(_appId, entityGuid, ContentType.StaticName, new Dictionary<string, object>());
            ImportEntities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Create a xml import. The data stream passed will be imported to memory, and checked 
        /// for errors. If no error could be found, the data can be persisted to the repository.
        /// </summary>
        /// <param name="zoneId">ID of 2SexyContent zone</param>
        /// <param name="appId">ID of 2SexyContent application</param>
        /// <param name="contentTypeId">ID of 2SexyContent type</param>
        /// <param name="dataStream">Xml data stream to import</param>
        /// <param name="languages">Languages that can be imported (2SexyContent languages enabled)</param>
        /// <param name="documentLanguageFallback">Fallback document language</param>
        /// <param name="deleteSetting">How to handle entities already in the repository</param>
        /// <param name="resolveReferenceMode">How value references to files and pages are handled</param>
        public ToRefactorXmlImportVTable(int zoneId, int appId, int contentTypeId, Stream dataStream, IEnumerable<string> languages, string documentLanguageFallback, ImportDeleteUnmentionedItems deleteSetting, ImportResourceReferenceMode resolveReferenceMode, Log parentLog)
        {
            Log = new Log("XmlIVT", parentLog, "building xml vtable import");
            ImportEntities = new List<Entity>();
            ErrorLog = new ImportErrorLog();

            _appId = appId;
            _zoneId = zoneId;
            DbContext = DbDataController.Instance(zoneId, appId, Log);

            ContentType = DbContext.AttribSet.GetDbAttribSet(contentTypeId);
            if (ContentType == null)
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidContentType);
                return;
            }

            AttributesOfType = DbContext.AttributesDefinition.GetAttributeDefinitions(contentTypeId).ToList();
            ExistingEntities = DbContext.Entities.GetEntitiesByType(ContentType).ToList();

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
        private DbDataController DbContext { get; }
        private List<ToSicEavAttributes> AttributesOfType { get; }
        private List<ToSicEavEntities> ExistingEntities { get; }

        private ToSicEavEntities FindInExisting(Guid guid)
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
                    var valueName = attribute.StaticName;
                    var value = documentElement.Element(valueName)?.Value;
                    if (value == null || value == XmlConstants.Null /* "[]" */) // value.IsValueNull())
                        continue;

                    if (value == XmlConstants.Empty /* "[\"\"]" */) //value.IsValueEmpty())
                    {
                        // It is an empty string
                        entity.Attributes.AddValue(valueName, "", attribute.Type, documentElementLanguage, false,
                            _resolveReferenceMode == ImportResourceReferenceMode.Resolve);
                        continue;
                    }

                    var valueReferenceLanguage = value.GetLanguageInARefTextCode();
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
                    if (valueReferenceProtection != XmlConstants.ReadWrite /* "rw" */ && valueReferenceProtection != XmlConstants.ReadOnly /* "ro" */)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReferenceProtection, value,
                            documentElementNumber);
                        continue;
                    }
                    var valueReadOnly = valueReferenceProtection == XmlConstants.ReadOnly /* "ro" */;

                    // if this value is just a placeholder/reference to another value,
                    // then find the master record, and add this language to it's users
                    var entityValue = entity.Attributes.FindItemOfLanguage(valueName, valueReferenceLanguage);
                    if (entityValue != null)
                    {
                        entityValue.Languages.Add(new Dimension { Key = documentElementLanguage, ReadOnly = valueReadOnly });
                        continue;
                    }

                    // We do not have the value referenced in memory, so search for the 
                    // value in the database 
                    var dbEntity = FindInExisting(entityGuid); // _contentType.EntityByGuid(entityGuid);
                    if (dbEntity == null)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReference, value, documentElementNumber);
                        continue;
                    }

                    var dbEntityValue = dbEntity.GetValueOfExactLanguage(attribute, valueReferenceLanguage);
                    if (dbEntityValue == null)
                    {
                        ErrorLog.AppendError(ImportErrorCode.InvalidValueReference, value, documentElementNumber);
                        continue;
                    }

                    entity.Attributes.AddValue(valueName, dbEntityValue.Value, valueType, valueReferenceLanguage,
                            dbEntityValue.IsLanguageReadOnly(valueReferenceLanguage),
                            _resolveReferenceMode == ImportResourceReferenceMode.Resolve)
                        //.AddLanguageReference(documentElementLanguage, valueReadOnly);
                        .Languages.Add(new Dimension { Key = documentElementLanguage, ReadOnly = valueReadOnly });
                }
            }

        }

        private bool RunDocumentValidityChecks()
        {
            // Assure that each element has a GUID and language child element
            foreach (var element in DocumentElements)
            {
                if (element.Element(XmlConstants.EntityGuid) == null)
                    element.Add(new XElement(XmlConstants.EntityGuid, ""));
                if (element.Element(XmlConstants.EntityLanguage) == null)
                    element.Add(new XElement(XmlConstants.EntityLanguage, ""));
            }

            var documentElementLanguagesAll = DocumentElements
                .GroupBy(element => element.Element(XmlConstants.EntityGuid)?.Value)
                .Select(group => @group
                    .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value)
                    .ToList())
                .ToList();

            var documentElementLanguagesCount = documentElementLanguagesAll.Select(item => item.Count);

            if (documentElementLanguagesCount.Any(count => count != 1))
                // It is an all language import, so check if all languages are specified for all entities
                if (documentElementLanguagesAll.Any(lang => _languages.Except(lang).Any()))
                {
                    ErrorLog.AppendError(ImportErrorCode.MissingElementLanguage,
                        "Langs=" + string.Join(", ", _languages));
                    return false;
                }
            return true;
        }

        private bool LoadStreamIntoDocumentElement(Stream dataStream)
        {
            Document = XDocument.Load(dataStream);
            dataStream.Position = 0;
            if (Document == null)
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidDocument);
                return false;
            }

            var documentRoot = Document.Element(XmlConstants.Root);
            if (documentRoot == null)
                throw new Exception("can't import - document doesn't have a root element");

            DocumentElements = documentRoot.Elements(XmlConstants.Entity).ToList();
            if (!DocumentElements.Any())
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidDocument);
                return false;
            }

            // Check the content type of the document (it can be found on each element in the Type attribute)
            var documentTypeAttribute = DocumentElements.First().Attribute(XmlConstants.EntityTypeAttribute);
            if (documentTypeAttribute?.Value == null ||
                documentTypeAttribute.Value != ContentType.Name.RemoveSpecialCharacters())
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidRoot);
                return false;
            }

            return true;
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

            if (_deleteSetting == ImportDeleteUnmentionedItems.All)
            {
                var entityDeleteGuids = GetEntityDeleteGuids();
                foreach(var entityGuid in entityDeleteGuids)
                {
                    var entityId = FindInExisting(entityGuid).EntityId;
                    if (DbContext.Entities.CanDeleteEntity(entityId).Item1)
                        DbContext.Entities.DeleteEntity(entityId);
                }
            }

            Timer.Start();
            var import = new Import(_zoneId, _appId, false);
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

        // todo: warning: 2017-06-12 2dm - I changed this a bit, must check for side-effects
        private List<Guid> GetCreatedEntityGuids() 
            => ImportEntities.Select(entity => entity.EntityGuid != Guid.Empty ? entity.EntityGuid : Guid.NewGuid()).ToList();

        /// <summary>
        /// Get the languages found in the xml document.
        /// </summary>
        public IEnumerable<string> LanguagesInDocument => DocumentElements
            .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value)
            .Distinct();

        /// <summary>
        /// Get the attribute names in the xml document.
        /// </summary>
        public IEnumerable<string> AttributeNamesInDocument => DocumentElements.SelectMany(element => element.Elements())
            .GroupBy(attribute => attribute.Name.LocalName)
            .Select(group => @group.Key)
            .Where(name => name != XmlConstants.EntityGuid && name != XmlConstants.EntityLanguage)
            .ToList();

        /// <summary>
        /// The amount of enities created in the repository on data import.
        /// </summary>
        public int AmountOfEntitiesCreated
        {
            get
            {
                var existingGuids = GetExistingEntityGuids();
                var createdGuids = GetCreatedEntityGuids();
                return createdGuids.Except(existingGuids).Count();
            }          
        }

        /// <summary>
        /// The amount of enities updated in the repository on data import.
        /// </summary>
        public int AmountOfEntitiesUpdated
        {
           get 
           {
               var existingGuids = GetExistingEntityGuids();
               var createdGuids = GetCreatedEntityGuids();
               return createdGuids.Count(guid => existingGuids.Contains(guid));
           }
        }

        private List<Guid> GetEntityDeleteGuids()
        {
            var existingGuids = GetExistingEntityGuids();
            var createdGuids = GetCreatedEntityGuids();
            return existingGuids.Except(createdGuids).ToList();
        }
        
        /// <summary>
        /// The amount of enities deleted in the repository on data import.
        /// </summary>
        public int AmountOfEntitiesDeleted => _deleteSetting == ImportDeleteUnmentionedItems.None ? 0 : GetEntityDeleteGuids().Count;

        /// <summary>
        /// Get the attribute names in the content type.
        /// </summary>
        public IEnumerable<string> AttributeNamesInContentType 
            => AttributesOfType /*_contentType.ToSicEavAttributesInSets*/.Select(item => item.StaticName).ToList();
        //_contentType.GetStaticNames();

        /// <summary>
        /// Get the attributes not imported (ignored) from the document to the repository.
        /// </summary>
        public IEnumerable<string> AttributeNamesNotImported
        {
            get
            {
                var existingAttributes = AttributeNamesInContentType;//_contentType.GetStaticNames();
                var creatdAttributes = AttributeNamesInDocument;
                return existingAttributes.Except(creatdAttributes);
            }            
        }

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
