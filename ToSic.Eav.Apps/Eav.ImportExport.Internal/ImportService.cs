﻿using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Generics;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc;
using Entity = ToSic.Eav.Data.Entity;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Internal;

/// <summary>
/// Import Content Types and/or Entities to the EAV SqlStore
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImportService(
    Generator<DbDataController> genDbDataController,
    IImportExportEnvironment importExportEnvironment,
    LazySvc<EntitySaver> entitySaverLazy,
    DataBuilder dataBuilder)
    : ServiceBase("Eav.Import", connect: [genDbDataController, importExportEnvironment, entitySaverLazy, dataBuilder])
{
    #region Constructor / DI

    private const int ChunkLimitToStartChunking = 2500;
    private const int ChunkSizeAboveLimit = 500;


    public ImportService Init(int? zoneId, int appId, bool skipExistingAttributes, bool preserveUntouchedAttributes, int? parentAppId = default)
    {
        // Get the DB controller - it can handle zoneId being null
        // It's important to not use AppWorkContext or similar, because that would
        // try to load the App into cache, and initialize the App before it's fully imported
        var dbController = genDbDataController.New().Init(zoneId, appId, parentAppId);
        Storage = dbController;
        AppId = appId;
        ZoneId = dbController.ZoneId;

        SaveOptions = importExportEnvironment.SaveOptions(ZoneId) with
        {
            SkipExistingAttributes = skipExistingAttributes,
            PreserveUntouchedAttributes = preserveUntouchedAttributes,
        };

        return this;
    }

    #endregion

    #region Private Fields

    internal IStorage Storage;
    public SaveOptions SaveOptions;

    private int AppId;
    private int ZoneId;

    #endregion

    /// <summary>
    /// Import AttributeSets and Entities
    /// </summary>
    public void ImportIntoDb(IList<IContentType> newTypes, IList<Entity> newEntities) 
    {
        var l = Log.Fn($"types: {newTypes?.Count}; entities: {newEntities?.Count}", timer: true);
        Storage.DoWithDelayedCacheInvalidation(() =>
        {
            #region import AttributeSets if any were included but rollback transaction if necessary

            if (newTypes == null)
                l.A("No types to import");
            else
                Storage.DoInTransaction(() =>
                {
                    // get initial data state for further processing, content-typed definitions etc.
                    // important: must always create a new loader, because it will cache content-types which hurts the import
                    Storage.DoWhileQueuingVersioning(() =>
                    {
                        var nonSysTypes = Log.Func(message: "Import Types in Sys-Scope", timer: true, func: () =>
                        {
                            // load everything, as content-type metadata is normal entities
                            // but disable initialized, as this could cause initialize stuff we're about to import
                            var appReaderRaw = Storage.Loader.AppStateBuilderRaw(AppId, new()).Reader;
                            var newTypeList = newTypes.ToList();
                            // first: import the attribute sets in the system scope, as they may be needed by others...
                            // ...and would need a cache-refresh before 
                            // 2020-07-10 2dm changed to use StartsWith, as we have more scopes now
                            // before var sysAttributeSets = newSetsList.Where(a => a.Scope == Constants.ScopeSystem).ToList();
                            // warning: this may not be enough, we may have to always import the fields-scope first...
                            var newSysTypes = newTypeList
                                .Where(a => a.Scope?.StartsWith(Scopes.System) ?? false).ToList();
                            if (newSysTypes.Any())
                                MergeAndSaveContentTypes(appReaderRaw, newSysTypes);

                            return newTypeList.Where(a => !newSysTypes.Contains(a)).ToList();
                        });

                        var lInner = Log.Fn(message: "Import Types in non-Sys scopes", timer: true);
                        if (nonSysTypes.Any())
                        {
                            // now reload the app state as it has new content-types
                            // and it may need these to load the remaining attributes of the content-types
                            var appReaderRaw = Storage.Loader.AppStateBuilderRaw(AppId, new()).Reader;

                            // now the remaining attributeSets
                            MergeAndSaveContentTypes(appReaderRaw, nonSysTypes);
                        }
                        lInner.Done();
                    });
                });

            #endregion

            #region import Entities, but rollback transaction if necessary

            if (newEntities == null)
                l.A("Not entities to import");
            else
            {
                var appStateTemp = Storage.Loader.AppStateBuilderRaw(AppId, new()).Reader; // load all entities
                var newIEntitiesRaw = Log.Func(message: "Pre-Import Entities merge", timer: true, func: () =>
                    newEntities
                        .Select(entity => CreateMergedForSaving(entity, appStateTemp, SaveOptions))
                        .Where(e => e != null)
                        .Cast<IEntity>()
                        .ToList());

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
                        Storage.DoInTransaction(() => Storage.Save(chunk, SaveOptions));
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
        var l = Log.Fn(timer: true);
        // Here's the problem! #badmergeofmetadata
        var toUpdate = contentTypes.Select(type => MergeContentTypeUpdateWithExisting(appReader, type));
        var so = importExportEnvironment.SaveOptions(ZoneId) with
        {
            DiscardAttributesNotInType = true,
        };
        Storage.Save(toUpdate.ToList(), so);
        l.Done();
    }


    private List<IEntity> MetadataWithResetIds(IMetadataOf metadata)
    {
        return metadata.Concat(metadata.Permissions.Select(p => p.Entity))
            .Select(e => dataBuilder.Entity.CreateFrom(e, id: 0, repositoryId: 0, guid: Guid.NewGuid()))
            .ToList();
    }

    private IContentType MergeContentTypeUpdateWithExisting(IAppReader appReader, IContentType contentType) => Log.Func(l =>
    {

        l.A("New CT, must reset attributes");

        // Is it an update or new?
        var existing = appReader.GetContentType(contentType.NameId);
        if (existing == null)
        {
            // must ensure that attribute Metadata is officially seen as new
            // but the import data could have an Id, so we must reset it here.
            var newAttributes = contentType.Attributes.Select(a =>
                {
                    var attributeMetadata = MetadataWithResetIds(a.Metadata);
                    return dataBuilder.TypeAttributeBuilder.CreateFrom(a, metadataItems: attributeMetadata);
                })
                .ToList();


            var ctMetadata = MetadataWithResetIds(contentType.Metadata);
            var newType = dataBuilder.ContentType.CreateFrom(contentType, metadataItems: ctMetadata,
                attributes: newAttributes);

            return (newType, "existing not found, only reset IDs");
        }

        l.A("found existing, will merge");

        var mergedAttributes = contentType.Attributes.Select(newAttribute =>
            {
                var oldAttr = existing.Attributes.FirstOrDefault(a => a.Name == newAttribute.Name);
                if (oldAttr == null)
                {
                    l.A($"New attr {newAttribute.Name} not found on original, merge not needed");
                    return newAttribute;
                }

                var newMetaList = newAttribute.Metadata
                    .Select(impMd => MergeOneMd(appReader.Metadata, (int)TargetTypes.Attribute, oldAttr.AttributeId, impMd))
                    .ToList();

                if (newAttribute.Metadata.Permissions.Any())
                    newMetaList.AddRange(newAttribute.Metadata.Permissions.Select(p => p.Entity));
                return dataBuilder.TypeAttributeBuilder.CreateFrom(newAttribute, metadataItems: newMetaList);
            })
            .ToList();

        // check if the content-type has metadata, which needs merging
        var merged = contentType.Metadata
            .Select(impMd => MergeOneMd(appReader.Metadata, (int)TargetTypes.ContentType, contentType.NameId, impMd))
            .ToList();
        merged.AddRange(contentType.Metadata.Permissions.Select(p => p.Entity));

        var newContentType = dataBuilder.ContentType.CreateFrom(contentType, metadataItems: merged, attributes: mergedAttributes);
        // contentType.Metadata.Use(merged);

        return (newContentType, "done");
    });

    private IEntity MergeOneMd<T>(IMetadataSource appState, int mdType, T key, IEntity newMd)
    {
        var existingMetadata = appState.GetMetadata(mdType, key, newMd.Type.NameId).FirstOrDefault();
        if (existingMetadata == null)
            // Must Reset guid, reset, otherwise the save process assumes it already exists in the DB; NOTE: clone would be ok
            return dataBuilder.Entity.CreateFrom(newMd, guid: Guid.NewGuid(), id: 0);

        return entitySaverLazy.Value.CreateMergedForSaving(existingMetadata, newMd, SaveOptions);
    }


    /// <summary>
    /// Import an Entity with all values
    /// </summary>
    private IEntity CreateMergedForSaving<T>(IEntity update, T appState, SaveOptions saveOptions)
        where T : IAppReadEntities, IAppReadContentTypes
    {
        var l = Log.Fn<IEntity>();
        _mergeCountToStopLogging++;
        var logDetails = _mergeCountToStopLogging <= LogMaxMerges;
        if (_mergeCountToStopLogging == LogMaxMerges)
            l.A($"Hit {LogMaxMerges} merges, will stop logging details");

        #region try to get AttributeSet or otherwise cancel & log error

        var contentType = appState.GetContentType(update.Type.NameId);

        if (contentType == null) // AttributeSet not Found
        {
            Storage.ImportLogToBeRefactored.Add(new(EventLogEntryType.Error, $"ContentType not found for {update.Type.NameId}"));
            return l.ReturnNull("error");
        }

        // set type only if is not set yet 
        //if (update.Type.Id == 0)
        //    update.UpdateType(contentType);
        var typeReset = update.Type.Id != default ? update.Type : null;

        #endregion

        // Find existing Entities - meaning both draft and non-draft
        List<IEntity> existingEntities = null;
        if (update.EntityGuid != Guid.Empty)
            existingEntities = appState.List.Where(e => e.EntityGuid == update.EntityGuid).ToList();

        // Simplest case - nothing existing to update: return update-entity unchanged
        if (existingEntities == null || !existingEntities.Any())
            return l.Return(dataBuilder.Entity.CreateFrom(update, type: typeReset), "is new, nothing to merge, just set type to be sure");

        Storage.ImportLogToBeRefactored.Add(new(EventLogEntryType.Information,
            $"FYI: Entity {update.EntityId} already exists for guid {update.EntityGuid}"));

        // now update (main) entity id from existing - since it already exists
        var original = existingEntities.First();
        var result = entitySaverLazy.Value.CreateMergedForSaving(original, update, saveOptions, newId: original.EntityId, newType: typeReset, logDetails: logDetails);
        return l.Return(result, "ok");
    }

    private int _mergeCountToStopLogging;
    private const int LogMaxMerges = 100;
}