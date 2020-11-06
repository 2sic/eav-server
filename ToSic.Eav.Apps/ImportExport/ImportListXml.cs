using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Logging;
using Entity = ToSic.Eav.Data.Entity;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import a virtual table of content-items
    /// </summary>
    public partial class ImportListXml: HasLog 
    {
        private readonly Lazy<AttributeBuilder> _lazyAttributeBuilder;
        private readonly Lazy<Import> _importerLazy;
        private AttributeBuilder AttributeBuilder => _lazyAttributeBuilder.Value;
        private IContentType ContentType { get; set; }
        private List<IEntity> ExistingEntities { get; set; }

        private AppState App { get; set; }
        private AppManager AppMan { get; set; }

        public ImportListXml(Lazy<AttributeBuilder> lazyAttributeBuilder, Lazy<Import> importerLazy) : base("App.ImpVtT")
        {
            _lazyAttributeBuilder = lazyAttributeBuilder;
            _importerLazy = importerLazy;
        }

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
        /// <param name="resolveLinkMode">How value references to files and pages are handled</param>
        /// <param name="parentLog"></param>
        public ImportListXml Init(AppManager appMan,
            IContentType contentType,
            Stream dataStream, 
            IEnumerable<string> languages, 
            string documentLanguageFallback, 
            ImportDeleteUnmentionedItems deleteSetting, 
            ImportResolveReferenceMode resolveLinkMode, 
            ILog parentLog) // : base("App.ImpVT", parentLog, "building xml vtable import")
        {
            Log.LinkTo(parentLog);
            ImportEntities = new List<Entity>();
            ErrorLog = new ImportErrorLog(Log);

            AppMan = appMan;
            App = appMan.AppState;

            _appId = App.AppId;

            ContentType = contentType;
            if (ContentType == null)
            {
                ErrorLog.Add(ImportErrorCode.InvalidContentType);
                return this;
            }
            Log.Add("Content type ok:" + contentType.Name);

            ExistingEntities = App.List.Where(e => e.Type == contentType).ToList();
            Log.Add($"Existing entities: {ExistingEntities.Count}");

            _languages = languages?.ToList();
            if (_languages == null || !_languages.Any())
                _languages = new[] { string.Empty };

            _languages = _languages.Select(l => l.ToLowerInvariant()).ToList();
            _docLangPrimary = documentLanguageFallback.ToLowerInvariant();
            Log.Add($"Languages: {languages.Count()}, fallback: {_docLangPrimary}");
            _deleteSetting = deleteSetting;
            ResolveLinks = resolveLinkMode == ImportResolveReferenceMode.Resolve;

            Timer.Start();
            try
            {
                if (!LoadStreamIntoDocumentElement(dataStream)) return this;
                if (!RunDocumentValidityChecks()) return this;
                ValidateAndImportToMemory();
            }
            catch (Exception exception)
            {
                ErrorLog.Add(ImportErrorCode.Unknown, exception.ToString());
            }
            Timer.Stop();
            Log.Add($"Prep time: {Timer.ElapsedMilliseconds}ms");
            TimeForMemorySetup = Timer.ElapsedMilliseconds;
            
            return this;
        }

        /// <summary>
        /// Deserialize data xml stream to the memory. The data will also be checked for 
        /// errors.
        /// </summary>
        private bool ValidateAndImportToMemory()
        {
            var callLog = Log.Call<bool>(useTimer: true);
            var nodesCount = 0;
            var entityGuidManager = new ImportItemGuidManager();

            foreach (var xEntity in DocumentElements)
            {
                nodesCount++;

                var nodeLang = xEntity.Element(XmlConstants.EntityLanguage)?.Value.ToLowerInvariant();
                if (_languages.All(language => language != nodeLang))
                {
                    // problem when DNN does not support the language
                    ErrorLog.Add(ImportErrorCode.InvalidLanguage, $"Lang={nodeLang}", nodesCount);
                    continue;
                }

                var entityGuid = entityGuidManager.GetGuid(xEntity, _docLangPrimary);
                var entity = GetImportEntity(entityGuid) ?? AppendEntity(entityGuid);

                foreach (var attribute in ContentType.Attributes)
                {
                    var valType = attribute.Type;
                    var valName = attribute.Name;
                    var value = xEntity.Element(valName)?.Value;
                    if (value == null || value == XmlConstants.Null)
                        continue;

                    if (value == XmlConstants.Empty)
                    {
                        // It is an empty string
                        AttributeBuilder.AddValue(entity.Attributes, 
                        /*entity.Attributes.AddValue(*/valName, "", attribute.Type, nodeLang, false, ResolveLinks);
                        continue;
                    }

                    var valueReferenceLanguage = value.GetLanguageInARefTextCode()?.ToLowerInvariant();
                    if (valueReferenceLanguage == null) // It is not a value reference.. it is a normal text
                    {
                        try
                        {
                            AttributeBuilder.AddValue(entity.Attributes, 
                            /*entity.Attributes.AddValue(*/valName, value, valType, nodeLang, false, ResolveLinks);
                        }
                        catch (FormatException)
                        {
                            ErrorLog.Add(ImportErrorCode.InvalidValueFormat, $"{valName}:{valType}={value}", nodesCount);
                        }
                        continue;
                    }

                    var valueReferenceProtection = value.GetValueReferenceProtection();
                    if (valueReferenceProtection != XmlConstants.ReadWrite && valueReferenceProtection != XmlConstants.ReadOnly)
                    {
                        ErrorLog.Add(ImportErrorCode.InvalidValueReferenceProtection, value, nodesCount);
                        continue;
                    }
                    var valueReadOnly = valueReferenceProtection == XmlConstants.ReadOnly;

                    // if this value is just a placeholder/reference to another value,
                    // then find the master record, and add this language to it's users
                    var entityValue = entity.Attributes.FindItemOfLanguage(valName, valueReferenceLanguage);
                    if (entityValue != null)
                    {
                        entityValue.Languages.Add(new Language { Key = nodeLang, ReadOnly = valueReadOnly });
                        continue;
                    }

                    // so search for the value in the cache 
                    var existingEnt = FindInExisting(entityGuid);
                    if (existingEnt == null)
                    {
                        ErrorLog.Add(ImportErrorCode.InvalidValueReference, value, nodesCount);
                        continue;
                    }

                    var valExisting =
                        ExportImportValueConversion.GetExactAssignedValue(existingEnt[attribute.Name], valueReferenceLanguage, null);
                    if (valExisting == null)
                    {
                        ErrorLog.Add(ImportErrorCode.InvalidValueReference, value, nodesCount);
                        continue;
                    }

                    var val = AttributeBuilder.AddValue(entity.Attributes, /*entity.Attributes.AddValue(*/valName,
                            valExisting,
                            valType,
                            valueReferenceLanguage,
                            valExisting.Languages.FirstOrDefault(l => l.Key == valueReferenceLanguage)?.ReadOnly ?? false,
                            ResolveLinks);
                    val.Languages.Add(new Language {Key = nodeLang, ReadOnly = valueReadOnly});

                    Log.Add($"Nr. {nodesCount} ok");
                }
            }

            Log.Add($"Prepared {ImportEntities.Count} entities for import");
            return callLog("done", true);
        }


        /// <summary>
        /// Save the data in memory to the repository.
        /// </summary>
        /// <returns>True if succeeded</returns>
        public bool PersistImportToRepository()
        {
            var callLog = Log.Call<bool>(useTimer: true);
            if (ErrorLog.HasErrors) return false;

            Timer.Start();
            if (_deleteSetting == ImportDeleteUnmentionedItems.All)
            {
                var idsToDelete = GetEntityDeleteGuids().Select(g => FindInExisting(g).EntityId).ToList();
                AppMan.Entities.Delete(idsToDelete);
            }

            var import = _importerLazy.Value.Init(null, _appId, false, true, Log);
            import.ImportIntoDb(null, ImportEntities);
            // important note: don't purge cache here, but the caller MUST do this!

            Timer.Stop();
            TimeForDbImport = Timer.ElapsedMilliseconds;
            return callLog("ok", true);
        }
        
    }




}
