using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps.ImportExport.ImportHelpers;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;
using ToSic.Lib.DI;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Logging;
using ToSic.Lib.Services;
using static System.StringComparer;
using Entity = ToSic.Eav.Data.Entity;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import a virtual table of content-items
    /// </summary>
    public partial class ImportListXml: ServiceBase 
    {
        private readonly DataBuilder _builder;

        #region Dependency Injection

        private readonly LazySvc<Import> _importerLazy;

        public ImportListXml(LazySvc<Import> importerLazy, DataBuilder builder) : base("App.ImpVtT")
        {
            ConnectServices(
                _builder = builder,
                _importerLazy = importerLazy
            );
        }

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
        public ImportListXml Init(AppManager appMan,
            IContentType contentType,
            Stream dataStream, 
            IEnumerable<string> languages, 
            string documentLanguageFallback, 
            ImportDeleteUnmentionedItems deleteSetting, 
            ImportResolveReferenceMode resolveLinkMode)
        {
            //ImportEntities = new List<Entity>();
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
        private bool ValidateAndImportToMemory() => Log.Func(timer: true, func: l =>
        {
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
                var entityInImportQueue = GetImportEntity(entityGuid);
                var entityAttributes = _builder.Attribute.Mutable(entityInImportQueue?.Attributes);

                foreach (var ctAttribute in ContentType.Attributes)
                {
                    var valType = ctAttribute.Type;
                    var valName = ctAttribute.Name;
                    var value = xEntity.Element(valName)?.Value;

                    // Case 1: Nothing
                    if (value == null || value == XmlConstants.NullMarker)
                        continue;

                    // Case 2: Xml empty string
                    if (value == XmlConstants.EmptyMarker)
                    {
                        entityAttributes.TryGetValue(valName, out var existingAttr);
                        var emptyAttribute = _builder.Attribute.CreateOrUpdate(existingAttr, valName, "", ctAttribute.Type, nodeLang);
                        entityAttributes = _builder.Attribute.Replace(entityAttributes, emptyAttribute);
                        continue;
                    }

                    // Check if reference to another language like "[ref(en-US,ro)]"
                    var valueReferenceLanguage = AttributeLanguageImportHelper.GetLanguageInARefTextCode(value)
                        ?.ToLowerInvariant();

                    // Case 3: Not a reference, normal value
                    if (valueReferenceLanguage == null) // It is not a value reference.. it is a normal text
                    {
                        try
                        {
                            entityAttributes.TryGetValue(valName, out var existingAttr2);
                            var preConverted = _builder.Value.PreConvertReferences(value, ctAttribute.Type, ResolveLinks);
                            var valRefAttribute = _builder.Attribute.CreateOrUpdate(existingAttr2, valName, preConverted, valType, nodeLang);
                            entityAttributes = _builder.Attribute.Replace(entityAttributes, valRefAttribute);

                        }
                        catch (FormatException)
                        {
                            ErrorLog.Add(ImportErrorCode.InvalidValueFormat, $"{valName}:{valType}={value}",
                                nodesCount);
                        }

                        continue;
                    }

                    // Case 4: It is a reference - but Error - Reference without specific "ro" or "rw"
                    var valueReferenceProtection = AttributeLanguageImportHelper.GetValueReferenceProtection(value);
                    if (valueReferenceProtection != XmlConstants.ReadWrite &&
                        valueReferenceProtection != XmlConstants.ReadOnly)
                    {
                        ErrorLog.Add(ImportErrorCode.InvalidValueReferenceProtection, value, nodesCount);
                        continue;
                    }

                    var valueReadOnly = valueReferenceProtection == XmlConstants.ReadOnly;

                    // if this value is just a placeholder/reference to another value,
                    // then find the master/primary value, and add this language to it's language list
                    var entityValue =
                        AttributeLanguageImportHelper.ValueItemOfLanguageOrNull(entityAttributes, valName,
                            valueReferenceLanguage);
                    if (entityValue.Value != null)
                    {
                        // 2023-02-24 2dm #immutable
                        // 2023-02-28 2dm As of now we have to clone to update the languages, and replace on the values list
                        // In future, we should move immutability "up" so this would go into a queue for values to create the final entity
                        var updatedValue = entityValue.Value.Clone(entityValue.Value.Languages.ToImmutableList()
                            .Add(new Language(nodeLang, valueReadOnly)));
                        var newValues = _builder.Value.Replace(entityValue.Attribute.Values,
                            entityValue.Value, updatedValue);
                        var newAttribute = _builder.Attribute.Clone(entityValue.Attribute, newValues);
                        entityAttributes = _builder.Attribute.Replace(entityAttributes, newAttribute);
                        //entityValue.Attribute.Values = dummy - fix;
                        //entityValue.Attribute.Values.Remove(entityValue.Value);
                        //entityValue.Attribute.Values.Add(updatedValue);
                        continue;
                    }

                    // so search for the value in the cache 
                    var existingEnt = FindInExisting(entityGuid);
                    if (existingEnt == null)
                    {
                        ErrorLog.Add(ImportErrorCode.InvalidValueReference, value, nodesCount);
                        continue;
                    }

                    var attrExisting = existingEnt[ctAttribute.Name];
                    var valExisting = ExportImportValueConversion.GetExactAssignedValue(attrExisting,
                            valueReferenceLanguage, null);
                    if (valExisting == null)
                    {
                        ErrorLog.Add(ImportErrorCode.InvalidValueReference, value, nodesCount);
                        continue;
                    }

                    #region 2024-03-02 2dm Section I completely rewrote incl. old code
                    // Note that the purpose of this code is not very clear, so I have to guess a bit
                    // as I'm rewriting it.

                    // 2023-02-24 2dm - old code for reference if something fails - remove ca. 2023Q2
                    // Note that according to my analysis, it would not have done anything useful
                    // Internally it would have gotteth the IValue (valExisting) to create a node
                    // It would then try to create a Value<string> or whatever, but can't cast the IValue to the real thing
                    // I'll try to fix it below, but it's not sure if it will result in anything useful.
                    //var val = AttributeBuilder.Value.AddValue(entity.Attributes, valName,
                    //    valExisting,
                    //    valType,
                    //    valueReferenceLanguage,
                    //    valExisting.Languages.FirstOrDefault(l => l.Key == valueReferenceLanguage)?.ReadOnly ?? false,
                    //    ResolveLinks);
                    //val.Languages.Add(new Language(nodeLang, valueReadOnly));

                    // Just add the value. Note 2023-02-28 2dm - not exactly sure how/why, assume it's the final-no-errors case
                    var langShouldBeReadOnly = valExisting.Languages
                        .FirstOrDefault(lang => lang.Key == valueReferenceLanguage)?.ReadOnly ?? false;
                    var valueLanguages = _builder.Language.GetBestValueLanguages(valueReferenceLanguage, langShouldBeReadOnly)
                                         ?? new List<ILanguage>();
                    valueLanguages.Add(new Language(nodeLang, valueReadOnly));
                    // update languages on valExisting
                    var updatedValue2 = _builder.Value.Clone(valExisting, _builder.Language.Merge(valExisting.Languages, valueLanguages));
                    //var updatedValue2 = AttributeBuilder.Value.UpdateLanguages(valExisting, valueLanguages);
                    // TODO: update/replace value in existingEnt[attribute.Name]
                    var values2 = _builder.Value.Replace(attrExisting.Values, valExisting, updatedValue2);
                    var attribute2 = _builder.Attribute.Clone(attrExisting, values2);
                    entityAttributes = _builder.Attribute.Replace(entityAttributes, attribute2);


                    #endregion
                    l.A($"Nr. {nodesCount} ok");
                }

                // entityAttributes was now updated, so we will either update/clone the existing entity, or create a new one
                if (entityInImportQueue == null)
                    AppendEntity(entityGuid, entityAttributes);
                else
                {
                    // note: I'm not sure if this should ever happen, if the same entity already exists
                    // in the ImportEntities list. But because there is a check if it's already in there
                    // which was from 2017 or before, I'll leave it in for now
                    var entityClone = _builder.Entity.Clone(entityInImportQueue, attributes: _builder.Attribute.Create(entityAttributes));
                    ImportEntities.Remove(entityInImportQueue);
                    ImportEntities.Add(entityClone as Entity);
                }

            }

            l.A($"Prepared {ImportEntities.Count} entities for import");
            return (true, "done");
        });


        /// <summary>
        /// Save the data in memory to the repository.
        /// </summary>
        /// <returns>True if succeeded</returns>
        public void PersistImportToRepository() => Log.Do(timer: true, action: () =>
        {
            if (ErrorLog.HasErrors) return "stop, errors";

            Timer.Start();
            if (_deleteSetting == ImportDeleteUnmentionedItems.All)
            {
                var idsToDelete = GetEntityDeleteGuids().Select(g => FindInExisting(g).EntityId).ToList();
                AppMan.Entities.Delete(idsToDelete);
            }

            var import = _importerLazy.Value.Init(null, _appId, false, true);
            import.ImportIntoDb(null, ImportEntities);
            // important note: don't purge cache here, but the caller MUST do this!

            Timer.Stop();
            TimeForDbImport = Timer.ElapsedMilliseconds;
            return "ok";
        });

    }




}
