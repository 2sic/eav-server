using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.DI;
using ToSic.Eav.Generics;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Lib.Services;
using Entity = ToSic.Eav.Data.Entity;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import: ServiceBase
    {
        private readonly EntityBuilder _entityBuilder;
        private const int ChunkLimitToStartChunking = 2500;
        private const int ChunkSizeAboveLimit = 500;

        #region Constructor / DI

        public Import(LazySvc<AppManager> appManagerLazy, 
            IImportExportEnvironment importExportEnvironment,
            LazySvc<EntitySaver> entitySaverLazy,
            EntityBuilder entityBuilder
            ) : base("Eav.Import")
        {
            ConnectServices(
                _appManagerLazy = appManagerLazy,
                _importExportEnvironment = importExportEnvironment,
                _entitySaver = entitySaverLazy,
                _entityBuilder = entityBuilder
            );
        }
        private readonly LazySvc<AppManager> _appManagerLazy;
        private readonly IImportExportEnvironment _importExportEnvironment;
        private readonly LazySvc<EntitySaver> _entitySaver;


        public Import Init(int? zoneId, int appId, bool skipExistingAttributes, bool preserveUntouchedAttributes)
        {
            AppManager = zoneId.HasValue
                ? _appManagerLazy.Value.Init(new AppIdentity(zoneId.Value, appId))
                : _appManagerLazy.Value.Init(appId);
            Storage = AppManager.Storage;
            AppId = appId;
            ZoneId = AppManager.ZoneId;

            SaveOptions = _importExportEnvironment.SaveOptions(ZoneId);

            SaveOptions.SkipExistingAttributes = skipExistingAttributes;

            SaveOptions.PreserveUntouchedAttributes = preserveUntouchedAttributes;

            return this;
        }
        internal AppManager AppManager;

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
        public void ImportIntoDb(IList<IContentType> newTypes, IList<Entity> newEntities
        ) => Log.Do($"types: {newTypes?.Count}; entities: {newEntities?.Count}", timer: true, action: l =>
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
                                var appStateTemp = Storage.Loader.AppState(AppId, false);
                                var newTypeList = newTypes.ToList();
                                // first: import the attribute sets in the system scope, as they may be needed by others...
                                // ...and would need a cache-refresh before 
                                // 2020-07-10 2dm changed to use StartsWith, as we have more scopes now
                                // before var sysAttributeSets = newSetsList.Where(a => a.Scope == Constants.ScopeSystem).ToList();
                                // warning: this may not be enough, we may have to always import the fields-scope first...
                                var newSysTypes = newTypeList
                                    .Where(a => a.Scope?.StartsWith(Scopes.System) ?? false).ToList();
                                if (newSysTypes.Any())
                                    MergeAndSaveContentTypes(appStateTemp, newSysTypes);

                                return newTypeList.Where(a => !newSysTypes.Contains(a)).ToList();
                            });

                            Log.Do(message: "Import Types in non-Sys scopes", timer: true, action: () =>
                            {
                                if (!nonSysTypes.Any()) return;

                                // now reload the app state as it has new content-types
                                // and it may need these to load the remaining attributes of the content-types
                                var appStateTemp = Storage.Loader.AppState(AppId, false);

                                // now the remaining attributeSets
                                MergeAndSaveContentTypes(appStateTemp, nonSysTypes);
                            });
                        });
                    });

                #endregion

                #region import Entities, but rollback transaction if necessary

                if (newEntities == null)
                    l.A("Not entities to import");
                else
                {
                    var appStateTemp = Storage.Loader.AppState(AppId, false); // load all entities
                    var newIEntitiesRaw = Log.Func(message: "Pre-Import Entities merge", timer: true, func: () => newEntities
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
            })
        );

        private void MergeAndSaveContentTypes(AppState appState, List<IContentType> contentTypes) => Log.Do(timer: true, action: () =>
        {
            // Here's the problem! #badmergeofmetadata
            contentTypes.ForEach(type => MergeContentTypeUpdateWithExisting(appState, type));
            var so = _importExportEnvironment.SaveOptions(ZoneId);
            so.DiscardAttributesNotInType = true;
            Storage.Save(contentTypes.ToList(), so);
        });
        
        


        private bool MergeContentTypeUpdateWithExisting(AppState appState, IContentType contentType)
        {
            var callLog = Log.Fn<bool>();
            var existing = appState.GetContentType(contentType.NameId);

            Log.A("New CT, must reset attributes");
            // must ensure that attribute Metadata is officially seen as new
            // but the import data could have an Id, so we must reset it here.
            foreach (var attribute in contentType.Attributes)
            {
                foreach (var attributeMd in attribute.Metadata)
                    attributeMd.ResetEntityId();
                foreach (var permission in attribute.Metadata.Permissions)
                    permission.Entity.ResetEntityId();
            }
            foreach (var metadata in contentType.Metadata)
                metadata.ResetEntityId();

            if (existing == null)
                return callLog.ReturnTrue("existing not found, won't merge");

            Log.A("found existing, will merge");
            foreach (var newAttribute in contentType.Attributes)
            {
                var oldAttr = existing.Attributes.FirstOrDefault(a => a.Name == newAttribute.Name);
                if (oldAttr == null)
                {
                    Log.A($"New attr {newAttribute.Name} not found on original, merge not needed");
                    continue;
                }

                var newMetaList = newAttribute.Metadata
                    .Select(impMd => MergeOneMd(appState, (int)TargetTypes.Attribute, oldAttr.AttributeId, impMd))
                    .ToList();

                if(newAttribute.Metadata.Permissions.Any())
                    newMetaList.AddRange(newAttribute.Metadata.Permissions.Select(p => p.Entity));

                ((IMetadataInternals)newAttribute.Metadata).Use(newMetaList);
            }

            // check if the content-type has metadata, which needs merging
            var merged = contentType.Metadata
                .Select(impMd => MergeOneMd(appState, (int)TargetTypes.ContentType, contentType.NameId, impMd))
                .ToList();
            merged.AddRange(contentType.Metadata.Permissions.Select(p => p.Entity));
            contentType.Metadata.Use(merged);

            return callLog.ReturnTrue("done");
        }

        private IEntity MergeOneMd<T>(IMetadataSource appState, int mdType, T key, IEntity newMd)
        {
            var existingMetadata = appState.GetMetadata(mdType, key, newMd.Type.NameId).FirstOrDefault();
            if (existingMetadata == null)
            {
                //metadataToUse = newMd;
                // Important to reset, otherwise the save process assumes it already exists in the DB
                // NOTE: clone would be ok
                return _entityBuilder.ResetIdentifiers(newMd, newGuid: Guid.NewGuid(), newId: 0);
                //metadataToUse.ResetEntityId();
                //metadataToUse.SetGuid(Guid.NewGuid());
            }

            return _entitySaver.Value.CreateMergedForSaving(existingMetadata, newMd, SaveOptions);
        }


        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private IEntity CreateMergedForSaving(Entity update, AppState appState, SaveOptions saveOptions) => Log.Func(l =>
        {
            _mergeCountToStopLogging++;
            var logDetails = _mergeCountToStopLogging <= LogMaxMerges;
            if (_mergeCountToStopLogging == LogMaxMerges)
                l.A($"Hit {LogMaxMerges} merges, will stop logging details");

            #region try to get AttributeSet or otherwise cancel & log error

            var contentType = appState.GetContentType(update.Type.NameId);

            if (contentType == null) // AttributeSet not Found
            {
                Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Error, $"ContentType not found for {update.Type.NameId}"));
                return (null, "error");
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
                return (_entityBuilder.Clone(update, newType: typeReset), "is new, nothing to merge, just set type to be sure");

            Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Information,
                $"FYI: Entity {update.EntityId} already exists for guid {update.EntityGuid}"));

            // now update (main) entity id from existing - since it already exists
            var original = existingEntities.First();
            //update.ResetEntityId(original.EntityId);
            var result = _entitySaver.Value.CreateMergedForSaving(original, update, saveOptions, newId: original.EntityId, newType: typeReset, logDetails: logDetails);
            return (result, "ok");
        });

        private int _mergeCountToStopLogging;
        private const int LogMaxMerges = 100;
    }
}