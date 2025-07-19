using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Eav.Repositories.Sys;
using IMetadataSource = ToSic.Eav.Metadata.Sys.IMetadataSource;


namespace ToSic.Eav.ImportExport.Sys;

/// <summary>
/// Import Content Types and/or Entities to the EAV SqlStore
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportService(
    Generator<IStorage, StorageOptions> storageFactory,
    IImportExportEnvironment importExportEnvironment,
    LazySvc<EntitySaver> entitySaverLazy,
    DataBuilder dataBuilder,
    DataImportLogSettings logSettings)
    : ServiceBase("Eav.Import", connect: [storageFactory, importExportEnvironment, entitySaverLazy, dataBuilder, logSettings])
{
    #region Detailed Logging

    [field: AllowNull, MaybeNull]
    private LogSettings LogSettings { get; set; } = null!;

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    private ILog? LogDetails => field ??= Log.IfDetails(LogSettings);

    private ILog? LogSummary => field ??= Log.IfSummary(LogSettings);

    #endregion

    #region Constructor / DI

    private const int ChunkLimitToStartChunking = 25000;
    private const int ChunkSizeAboveLimit = 25000;


    public ImportService Init(int zoneId, int appId, bool skipExistingAttributes, bool preserveUntouchedAttributes, int? parentAppId = default)
    {
        // Get the DB controller - it can handle zoneId being null
        // It's important to not use AppWorkContext or similar, because that would
        // try to load the App into cache, and initialize the App before it's fully imported
        LogSettings = logSettings.GetLogSettings();
        var storage = storageFactory.New(new(zoneId, appId, parentAppId, LogSettings));
        Storage = storage;
        _appId = appId;
        _zoneId = zoneId;

        SaveOptions = importExportEnvironment.SaveOptions(_zoneId) with
        {
            SkipExistingAttributes = skipExistingAttributes,
            PreserveUntouchedAttributes = preserveUntouchedAttributes,
        };

        return this;
    }

    #endregion

    #region Private Fields

    internal IStorage Storage = null!;
    public SaveOptions SaveOptions = null!;

    private int _appId;
    private int _zoneId;

    #endregion

    /// <summary>
    /// Import Content-Types and Entities
    /// </summary>
    public void ImportIntoDb(IList<IContentType> newTypes, IList<Entity> newEntities) 
    {
        var l = LogSummary.Fn($"types: {newTypes.Count}; entities: {newEntities.Count}", timer: true);
        Storage.DoWithDelayedCacheInvalidation(() =>
        {
            #region import Content-Types if any were included but rollback transaction if necessary

            if (newTypes.Count == 0)
                l.A("No types to import");
            else
                Storage.DoInTransaction(() =>
                {
                    // get initial data state for further processing, content-typed definitions etc.
                    // important: must always create a new loader, because it will cache content-types which hurts the import
                    Storage.DoWhileQueuingVersioning(() =>
                    {
                        var nonSysTypes = LogSummary.Quick(message: "Import Types in Sys-Scope", timer: true, func: () =>
                        {
                            // load everything, as content-type metadata is normal entities
                            // but disable initialized, as this could cause initialize stuff we're about to import
                            var appReaderRaw = Storage.Loader.AppReaderRaw(_appId, new());
                            var newTypeList = newTypes.ToList();
                            // first: import the attribute sets in the system scope, as they may be needed by others...
                            // ...and would need a cache-refresh before 
                            var newSysTypes = newTypeList
                                .Where(a => a.Scope?.StartsWith(ScopeConstants.System) ?? false)
                                .ToList();
                            if (newSysTypes.Any())
                                MergeAndSaveContentTypes(appReaderRaw, newSysTypes);

                            return newTypeList
                                .Where(a => !newSysTypes.Contains(a))
                                .ToList();
                        });

                        var lInner = l.Fn(message: "Import Types in non-Sys scopes", timer: true);
                        if (nonSysTypes.Any())
                        {
                            // now reload the app state as it has new content-types
                            // and it may need these to load the remaining attributes of the content-types
                            var appReaderRaw = Storage.Loader.AppReaderRaw(_appId, new());

                            // now the remaining Content-Types
                            MergeAndSaveContentTypes(appReaderRaw, nonSysTypes);
                        }
                        lInner.Done();
                    });
                });

            #endregion

            #region import Entities, but rollback transaction if necessary

            if (newEntities.Count == 0)
                l.A("Not entities to import");
            else
            {
                var appStateTemp = Storage.Loader.AppReaderRaw(_appId, new()); // load all entities
                var newIEntitiesRaw = LogSummary.Quick(message: "Pre-Import Entities merge", timer: true,
                    func: () => newEntities
                        .Select(entity => CreateMergedForSaving(entity, appStateTemp, SaveOptions))
                        .Select(pair => pair?.Entity)
                        .Where(e => e != null)
                        .Cast<IEntity>()
                        .ToList()
                );

                // HACK 2022-05-05 2dm Import Problem
                // If we use chunks of 500, then relationships are not imported
                // in situations where the target is in a future chunk (because the relationship can't find a matching target item)
                // Large imports are rare in Apps, but on data-import it's common
                // For now the hack is to allow up to 2500 in one chunk, otherwise make them smaller
                // this is not a good final solution
                // In general we should improve the import to 
                var chunkSize = /*ChunkSizeAboveLimit;*/ newIEntitiesRaw.Count >= ChunkLimitToStartChunking
                    ? ChunkSizeAboveLimit
                    : ChunkLimitToStartChunking;

                var newIEntities = newIEntitiesRaw.ChunkBy(chunkSize);

                // Import in chunks
                var cNum = 0;
                // HACK 2022-05-05 2dm experimental, but not activated
                // This would be an idea to queue the relationships across all items, but then it will probably become very slow and if it fails, nothing is imported

                // Must queue relationships around everything, otherwise relationships in different chunks may not be found on chunk import
                //Storage.DoInTransaction(
                //    () => Storage.DoWhileQueueingRelationships(
                //        () => 
                newIEntities.ForEach(chunk =>
                    {
                        cNum++;
                        l.A($"Importing Chunk {cNum} #{(cNum - 1) * chunkSize + 1} - #{cNum * chunkSize}");
                        var withOptions = SaveOptions.AddToAll(chunk);
                        Storage.DoInTransaction(() => Storage.Save(withOptions));
                    })
                    //))
                    ;
            }

            #endregion
        });
        l.Done();
    }

    private void MergeAndSaveContentTypes(IAppReader appReader, List<IContentType> contentTypes)
    {
        var l = LogSummary.Fn(timer: true);
        // Here's the problem! #badmergeofmetadata
        var toUpdate = contentTypes.Select(type => MergeContentTypeUpdateWithExisting(appReader, type));
        var so = importExportEnvironment.SaveOptions(_zoneId) with
        {
            DiscardAttributesNotInType = true,
        };
        Storage.Save(toUpdate.ToList(), so);
        l.Done();
    }


    private List<IEntity> MetadataWithResetIds(IMetadata metadata)
    {
        return metadata.Concat(metadata.Permissions.Select(p => ((ICanBeEntity)p).Entity))
            .Select(e => dataBuilder.Entity.CreateFrom(e, id: 0, repositoryId: 0, guid: Guid.NewGuid()))
            .ToList();
    }

    private IContentType MergeContentTypeUpdateWithExisting(IAppReader appReader, IContentType contentType)
    {
        var l = LogDetails.Fn<IContentType>();

        l.A("New CT, must reset attributes");

        // Is it an update or new?
        var existing = appReader.TryGetContentType(contentType.NameId);
        if (existing == null)
        {
            // must ensure that attribute Metadata is officially seen as new
            // but the import data could have an Id, so we must reset it here.
            var newAttributes = contentType.Attributes
                .Select(a =>
                {
                    var attributeMetadata = MetadataWithResetIds(a.Metadata);
                    return dataBuilder.TypeAttributeBuilder.CreateFrom(a, metadataItems: attributeMetadata);
                })
                .ToList();


            var ctMetadata = MetadataWithResetIds(contentType.Metadata);
            var newType = dataBuilder.ContentType.CreateFrom(contentType, metadataItems: ctMetadata,
                attributes: newAttributes);

            return l.Return(newType, "existing not found, only reset IDs");
        }

        l.A("found existing, will merge");

        var mergedAttributes = contentType.Attributes
            .Select(newAttribute =>
            {
                var oldAttr = existing.Attributes.FirstOrDefault(a => a.Name == newAttribute.Name);
                if (oldAttr == null)
                {
                    l.A($"New attr {newAttribute.Name} not found on original, merge not needed");
                    return newAttribute;
                }

                var newMetaList = newAttribute.Metadata
                    .Select(impMd => MergeOneMetadata(appReader.Metadata, (int)TargetTypes.Attribute, oldAttr.AttributeId, impMd).Entity)
                    .ToList();

                if (newAttribute.Metadata.Permissions.Any())
                    newMetaList.AddRange(newAttribute.Metadata.Permissions.Select(p => ((ICanBeEntity)p).Entity));
                return dataBuilder.TypeAttributeBuilder.CreateFrom(newAttribute, metadataItems: newMetaList);
            })
            .ToList();

        // check if the content-type has metadata, which needs merging
        var merged = contentType.Metadata
            .Select(impMd => MergeOneMetadata(appReader.Metadata, (int)TargetTypes.ContentType, contentType.NameId, impMd).Entity)
            .ToList();
        merged.AddRange(contentType.Metadata.Permissions
            .Select(p => ((ICanBeEntity)p).Entity)
        );

        var newContentType = dataBuilder.ContentType.CreateFrom(contentType, metadataItems: merged, attributes: mergedAttributes);
        // contentType.Metadata.Use(merged);

        return l.Return(newContentType, "done");
    }

    private IEntityPair<SaveOptions> MergeOneMetadata<T>(IMetadataSource appState, int mdType, T key, IEntity newMd)
    {
        var existingMetadata = appState.GetMetadata(mdType, key, newMd.Type.NameId).FirstOrDefault();
        if (existingMetadata == null)
            // Must Reset guid, reset, otherwise the save process assumes it already exists in the DB; NOTE: clone would be ok
            return new EntityPair<SaveOptions>(dataBuilder.Entity.CreateFrom(newMd, guid: Guid.NewGuid(), id: 0), SaveOptions);

        return entitySaverLazy.Value.CreateMergedForSaving(existingMetadata, newMd, SaveOptions, logSettings: LogSettings);
    }


    /// <summary>
    /// Import an Entity with all values
    /// </summary>
    private IEntityPair<SaveOptions>? CreateMergedForSaving<T>(IEntity update, T appState, SaveOptions saveOptions)
        where T : IAppReadEntities, IAppReadContentTypes
    {
        var l = LogDetails.Fn<IEntityPair<SaveOptions>>();
        _mergeCountToStopLogging++;
        var logDetails = LogSettings.Details && _mergeCountToStopLogging <= LogMaxMerges;
        if (_mergeCountToStopLogging == LogMaxMerges)
            l.A($"Hit {LogMaxMerges} merges, will stop logging details");

        #region try to get Content-Type or otherwise cancel & log error

        var contentType = appState.TryGetContentType(update.Type.NameId);

        if (contentType == null) // not Found
        {
            Storage.ImportLogToBeRefactored.Add(new($"ContentType not found for {update.Type.NameId}", Message.MessageTypes.Error));
            return l.ReturnNull("error");
        }

        // set type only if is not set yet 
        //if (update.Type.Id == 0)
        //    update.UpdateType(contentType);
        var typeReset = update.Type.Id != default ? update.Type : null;

        #endregion

        // Find existing Entities - meaning both draft and non-draft
        List<IEntity>? existingEntities = null;
        if (update.EntityGuid != Guid.Empty)
            existingEntities = appState.List.Where(e => e.EntityGuid == update.EntityGuid).ToList();

        // Simplest case - nothing existing to update: return update-entity unchanged
        if (existingEntities == null || !existingEntities.Any())
        {
            var toCreate = dataBuilder.Entity.CreateFrom(update, type: typeReset);
            return l.Return(new EntityPair<SaveOptions>(toCreate, saveOptions) , "is new, nothing to merge, just set type to be sure");
        }

        Storage.ImportLogToBeRefactored.Add(new($"FYI: Entity {update.EntityId} already exists for guid {update.EntityGuid}", Message.MessageTypes.Information));

        // now update (main) entity id from existing - since it already exists
        var original = existingEntities.First();
        var result = entitySaverLazy.Value.CreateMergedForSaving(original, update, saveOptions, newId: original.EntityId, newType: typeReset, logSettings: LogSettings);
        return l.Return(result, "ok");
    }

    private int _mergeCountToStopLogging;
    private const int LogMaxMerges = 100;
}