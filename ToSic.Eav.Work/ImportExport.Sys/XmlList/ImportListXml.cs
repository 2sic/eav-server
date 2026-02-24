using System.Xml.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Eav.ImportExport.Sys.ImportHelpers;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Persistence.Sys.Logging;

namespace ToSic.Eav.ImportExport.Sys.XmlList;

/// <summary>
/// Import a virtual table of content-items
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class ImportListXml(
    LazySvc<ImportService> importerLazy,
    Generator<DataAssembler, DataAssemblerOptions> builderGenerator,
    LazySvc<IValueConverter> valueConverter,
    GenWorkDb<WorkEntityDelete> entDelete)
    : ServiceBase("App.ImpVtT", connect: [builderGenerator, valueConverter, importerLazy, entDelete])
{
    private DataAssembler DataAssembler { get; set; } = null!;

    #region Init

    private IAppReader AppReader { get; set; } = null!;

    #region Detailed Logging

    [field: AllowNull, MaybeNull]
    private LogSettings LogSettings { get; set; } = null!;

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    internal ILog? LogDetails => field ??= Log.IfDetails(LogSettings);

    internal ILog? LogSummary => field ??= Log.IfSummary(LogSettings);

    #endregion

    /// <summary>
    /// Create a xml import. The data stream passed will be imported to memory, and checked 
    /// for errors. If no error could be found, the data can be persisted to the repository.
    /// </summary>
    /// <param name="appReader"></param>
    /// <param name="typeName">content-type</param>
    /// <param name="dataStream">Xml data stream to import</param>
    /// <param name="languages">Languages that can be imported (2sxc languages enabled)</param>
    /// <param name="documentLanguageFallback">Fallback document language</param>
    /// <param name="deleteSetting">How to handle entities already in the repository</param>
    /// <param name="resolveLinkMode">How value references to files and pages are handled</param>
    /// <param name="logSettings"></param>
    public ImportListXml Init(IAppReader appReader,
        string typeName,
        Stream dataStream,
        IEnumerable<string> languages,
        string documentLanguageFallback,
        ImportDeleteUnmentionedItems deleteSetting,
        ImportResolveReferenceMode resolveLinkMode,
        LogSettings logSettings)
    {
        var langs = languages.ToList();
        var l = LogSummary.Fn<ImportListXml>($"type: {typeName}, langs: {langs.Count}, delete: {deleteSetting}, resolve: {resolveLinkMode}", timer: true);
        ErrorLog = new(Log);

        LogSettings = logSettings;
        DataAssembler = builderGenerator.New(new() { LogSettings = logSettings });
        AppReader = appReader;
        _deleteSetting = deleteSetting;
        ResolveLinks = resolveLinkMode == ImportResolveReferenceMode.Resolve;

        // Get Content Type and Exit if not found
        var contentType = appReader.TryGetContentType(typeName);
        if (contentType == null)
        {
            ErrorLog.Add(ImportErrorCode.InvalidContentType);
            return l.ReturnAsError(this, "content type not found");
        }
        l.A("Content type ok:" + contentType.Name);

        var existingEntities = appReader.List
            .Where(e => e.Type == contentType)
            .ToList();

        l.A($"Existing entities: {existingEntities.Count}");

        if (!langs.Any())
            langs = [string.Empty];

        ImportConfig = new()
        {
            DocLangPrimary = documentLanguageFallback.ToLowerInvariant(),
            Languages = langs
                .Select(lng => lng.ToLowerInvariant())
                .ToList(),
        };

        l.A($"Languages: {ImportConfig.Languages.Count}, fallback: {ImportConfig.DocLangPrimary}");

        Timer.Start();
        try
        {
            if (!LoadStreamIntoDocumentElement(contentType, dataStream, out var xmlEntities))
                return l.ReturnAsError(this, "couldn't load stream");
            if (!RunDocumentValidityChecks(xmlEntities))
                return l.ReturnAsError(this, "document didn't pass validity checks");
            
            // Since all is ok, we can now create the import stats
            ValidateAndImportToMemory(appReader.AppId, contentType, xmlEntities, existingEntities);

            // Note: this is not very clean yet, it relies on side effects from the ValidateAndImportToMemory method
            var existingGuids = GetExistingEntityGuids(existingEntities);
            var creatingGuids = GetCreatedEntityGuids(ImportEntities);
            Preparations = new(
                DocumentElements.ToList(),
                contentType,
                existingEntities,
                _deleteSetting, existingGuids, creatingGuids, GetEntityDeleteGuids(existingGuids, creatingGuids));

        }
        catch (Exception exception)
        {
            l.Ex(exception);
            ErrorLog.Add(ImportErrorCode.Unknown, exception.ToString());
        }
        Timer.Stop();
        l.A($"Prep time: {Timer.ElapsedMilliseconds}ms");
        TimeForMemorySetup = Timer.ElapsedMilliseconds;
        
            
        return l.ReturnAsOk(this);
    }

    #endregion

    /// <summary>
    /// Deserialize data xml stream to the memory. The data will also be checked for 
    /// errors.
    /// </summary>
    private bool ValidateAndImportToMemory(int appId, IContentType contentType, List<XElement> xmlEntities, List<IEntity> existingEntities)
    {
        var l = LogSummary.Fn<bool>(timer: true);
        var nodesCount = 0;
        var entityGuidManager = new ImportItemGuidManager();

        foreach (var xEntity in xmlEntities)
        {
            nodesCount++;

            var nodeLangRaw = xEntity.Element(XmlConstants.EntityLanguage)
                ?.Value
                .ToLowerInvariant();

            if (ImportConfig.Languages.All(language => language != nodeLangRaw))
            {
                // problem when DNN does not support the language
                ErrorLog.Add(ImportErrorCode.InvalidLanguage, $"Lang={nodeLangRaw}", nodesCount);
                continue;
            }

            var nodeLang = nodeLangRaw!;

            var entityGuid = entityGuidManager.GetGuid(xEntity, ImportConfig.DocLangPrimary);
            var entityInImportQueue = GetImportEntity(entityGuid);
            var entityAttributes = DataAssembler.AttributeList.ConvertToMutable(entityInImportQueue?.Attributes);

            foreach (var ctAttribute in contentType.Attributes)
            {
                var valType = ctAttribute.Type;
                var valName = ctAttribute.Name;
                var value = xEntity.Element(valName)?.Value;

                // Case 1: Nothing
                if (value is null or XmlConstants.NullMarker)
                    continue;

                // Case 2: Xml empty string
                if (value == XmlConstants.EmptyMarker)
                {
                    entityAttributes.TryGetValue(valName, out var existingAttr);
                    var emptyAttribute = DataAssembler.Attribute.CreateOrUpdate(originalOrNull: existingAttr, name: valName, value: "", type: ctAttribute.Type, language: nodeLang);
                    entityAttributes = DataAssembler.AttributeList.Replace(entityAttributes, emptyAttribute);
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
                        var preConverted = valueConverter.Value.PreConvertReferences(value, ctAttribute.Type, ResolveLinks);
                        var valRefAttribute = DataAssembler.Attribute.CreateOrUpdate(originalOrNull: existingAttr2, name: valName, value: preConverted, type: valType, language: nodeLang);
                        entityAttributes = DataAssembler.AttributeList.Replace(entityAttributes, valRefAttribute);

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
                // then find the master/primary value, and add this language to its language list
                var entityValue = AttributeLanguageImportHelper
                    .ValueItemOfLanguageOrNull(entityAttributes, valName, valueReferenceLanguage);
                if (entityValue.Value != null)
                {
                    // 2023-02-24 2dm #immutable
                    // 2023-02-28 2dm As of now we have to clone to update the languages, and replace on the values list
                    // In the future, we should move immutability "up" so this would go into a queue for values to create the final entity
                    var updatedValue = entityValue.Value.With(
                        entityValue.Value.Languages.ToImmutableOpt()
                            .Add(new Language(nodeLang, valueReadOnly))
                    );
                    var newValues = DataAssembler.ValueList.Replace(entityValue.Attribute!.Values,
                        entityValue.Value, updatedValue);
                    var newAttribute = DataAssembler.Attribute.CreateFrom(entityValue.Attribute, newValues);
                    entityAttributes = DataAssembler.AttributeList.Replace(entityAttributes, newAttribute);
                    continue;
                }

                // so search for the value in the cache 
                var existingEnt = existingEntities.GetOne(entityGuid);
                if (existingEnt == null)
                {
                    ErrorLog.Add(ImportErrorCode.InvalidValueReference, value, nodesCount);
                    continue;
                }

                var attrExisting = existingEnt[ctAttribute.Name]!;
                var valExisting = ExportImportValueConversion.GetExactAssignedValue(attrExisting, valueReferenceLanguage, null! /* ignore language fallback */);
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
                var valueLanguages = DataAssembler.Language.GetBestValueLanguages(valueReferenceLanguage, langShouldBeReadOnly)
                                     ?? [];
                valueLanguages.Add(new Language(nodeLang, valueReadOnly));
                // update languages on valExisting
                var updatedValue2 = DataAssembler.Value.CreateFrom(valExisting, languages: DataAssembler.Language.Merge(valExisting.Languages, valueLanguages));
                //var updatedValue2 = AttributeBuilder.Value.UpdateLanguages(valExisting, valueLanguages);
                // TODO: update/replace value in existingEnt[attribute.Name]
                var values2 = DataAssembler.ValueList.Replace(attrExisting.Values, valExisting, updatedValue2);
                var attribute2 = DataAssembler.Attribute.CreateFrom(attrExisting, values2);
                entityAttributes = DataAssembler.AttributeList.Replace(entityAttributes, attribute2);


                #endregion
                l.A($"Nr. {nodesCount} ok");
            }

            // entityAttributes was now updated, so we will either update/clone the existing entity, or create a new one
            if (entityInImportQueue == null)
                AppendEntity(appId, contentType, entityGuid, entityAttributes);
            else
            {
                // note: I'm not sure if this should ever happen, if the same entity already exists
                // in the ImportEntities list. But because there is a check if it's already in there
                // which was from 2017 or before, I'll leave it in for now
                var entityClone = DataAssembler.Entity.CreateFrom(entityInImportQueue, attributes: DataAssembler.AttributeList.Finalize(entityAttributes));
                ImportEntities.Remove(entityInImportQueue);
                ImportEntities.Add((Entity)entityClone);
            }

        }

        l.A($"Prepared {ImportEntities.Count} entities for import");
        return l.ReturnTrue("done");
    }


    /// <summary>
    /// Save the data in memory to the repository.
    /// </summary>
    /// <returns>True if succeeded</returns>
    public void PersistImportToRepository()
    {
        var l = LogSummary.Fn(timer: true);

        if (ErrorLog.HasErrors)
        {
            l.Done("stop, errors");
            return;
        }

        var appReader = AppReader;
        Timer.Start();
        if (_deleteSetting == ImportDeleteUnmentionedItems.All)
        {
            var idsToDelete = Preparations.EntityDeleteGuids
                .Select(g => Preparations.ExistingEntities.GetOne(g)!.EntityId)
                .ToList();
            entDelete.New(appReader).Delete(idsToDelete);
        }

        var import = importerLazy.Value.Init(appReader.ZoneId, appReader.AppId, false, true);
        import.ImportIntoDb([], ImportEntities);

        // important note: don't purge cache here, but the caller MUST do this!
        Timer.Stop();
        TimeForDbImport = Timer.ElapsedMilliseconds;
        l.Done("ok");
    }

}