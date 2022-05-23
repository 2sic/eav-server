﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps.ImportExport.ImportHelpers;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Plumbing;
using Entity = ToSic.Eav.Data.Entity;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import a virtual table of content-items
    /// </summary>
    public partial class ImportListXml: HasLog 
    {
        #region Dependency Injection

        private readonly Lazy<Import> _importerLazy;

        public ImportListXml(LazyInitLog<AttributeBuilderForImport> lazyAttributeBuilder, Lazy<Import> importerLazy) : base("App.ImpVtT")
        {
            AttributeBuilder = lazyAttributeBuilder.SetLog(Log);
            _importerLazy = importerLazy;
        }

        private readonly LazyInitLog<AttributeBuilderForImport> AttributeBuilder;


        #endregion

        #region Init

        private IContentType ContentType { get; set; }
        private List<IEntity> ExistingEntities { get; set; }

        private AppState App { get; set; }
        private AppManager AppMan { get; set; }


        /// <summary>
        /// Create a xml import. The data stream passed will be imported to memory, and checked 
        /// for errors. If no error could be found, the data can be persisted to the repository.
        /// </summary>
        /// <param name="appMan"></param>
        /// <param name="contentType">content-type</param>
        /// <param name="dataStream">Xml data stream to import</param>
        /// <param name="languages">Languages that can be imported (2sxc languages enabled)</param>
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
            ILog parentLog)
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
            Log.A("Content type ok:" + contentType.Name);

            ExistingEntities = App.List.Where(e => e.Type == contentType).ToList();
            Log.A($"Existing entities: {ExistingEntities.Count}");

            _languages = languages?.ToList();
            if (_languages == null || !_languages.Any())
                _languages = new[] { string.Empty };

            _languages = _languages.Select(l => l.ToLowerInvariant()).ToList();
            _docLangPrimary = documentLanguageFallback.ToLowerInvariant();
            Log.A($"Languages: {languages.Count()}, fallback: {_docLangPrimary}");
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
            Log.A($"Prep time: {Timer.ElapsedMilliseconds}ms");
            TimeForMemorySetup = Timer.ElapsedMilliseconds;
            
            return this;
        }

        #endregion

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

                    // Case 1: Nothing
                    if (value == null || value == XmlConstants.Null)
                        continue;

                    // Case 2: Xml empty string
                    if (value == XmlConstants.Empty)
                    {
                        AttributeBuilder.Ready.AddValue(entity.Attributes, valName, "", attribute.Type, nodeLang, false, ResolveLinks);
                        continue;
                    }

                    // Check if reference to another language like "[ref(en-US,ro)]"
                    var valueReferenceLanguage = AttributeLanguageImportHelper.GetLanguageInARefTextCode(value)?.ToLowerInvariant();

                    // Case 3: Not a reference, normal value
                    if (valueReferenceLanguage == null) // It is not a value reference.. it is a normal text
                    {
                        try
                        {
                            AttributeBuilder.Ready.AddValue(entity.Attributes, valName, value, valType, nodeLang, false, ResolveLinks);
                        }
                        catch (FormatException)
                        {
                            ErrorLog.Add(ImportErrorCode.InvalidValueFormat, $"{valName}:{valType}={value}", nodesCount);
                        }
                        continue;
                    }

                    // Case 4: Error - Reference without specific "ro" or "rw"
                    var valueReferenceProtection = AttributeLanguageImportHelper.GetValueReferenceProtection(value);
                    if (valueReferenceProtection != XmlConstants.ReadWrite && valueReferenceProtection != XmlConstants.ReadOnly)
                    {
                        ErrorLog.Add(ImportErrorCode.InvalidValueReferenceProtection, value, nodesCount);
                        continue;
                    }
                    var valueReadOnly = valueReferenceProtection == XmlConstants.ReadOnly;

                    // if this value is just a placeholder/reference to another value,
                    // then find the master record, and add this language to it's users
                    var entityValue = AttributeLanguageImportHelper.ValueItemOfLanguageOrNull(entity.Attributes, valName, valueReferenceLanguage);
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

                    var val = AttributeBuilder.Ready.AddValue(entity.Attributes, valName,
                            valExisting,
                            valType,
                            valueReferenceLanguage,
                            valExisting.Languages.FirstOrDefault(l => l.Key == valueReferenceLanguage)?.ReadOnly ?? false,
                            ResolveLinks);
                    val.Languages.Add(new Language {Key = nodeLang, ReadOnly = valueReadOnly});

                    Log.A($"Nr. {nodesCount} ok");
                }
            }

            Log.A($"Prepared {ImportEntities.Count} entities for import");
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
