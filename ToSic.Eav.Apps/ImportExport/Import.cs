using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Plumbing;
using Entity = ToSic.Eav.Data.Entity;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import: HasLog
    {
        private const int ChunkLimitToStartChunking = 2500;
        private const int ChunkSizeAboveLimit = 500;

        #region Constructor / DI

        public Import(Lazy<AppManager> appManagerLazy, 
            IImportExportEnvironment importExportEnvironment,
            LazyInitLog<EntitySaver> entitySaverLazy
            ) : base("Eav.Import")
        {
            _appManagerLazy = appManagerLazy;
            _importExportEnvironment = importExportEnvironment;
            _entitySaver = entitySaverLazy.SetLog(Log);
        }
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly IImportExportEnvironment _importExportEnvironment;
        private readonly LazyInitLog<EntitySaver> _entitySaver;


        public Import Init(int? zoneId, int appId, bool skipExistingAttributes, bool preserveUntouchedAttributes, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            AppManager = zoneId.HasValue
                ? _appManagerLazy.Value.Init(new AppIdentity(zoneId.Value, appId), Log)
                : _appManagerLazy.Value.Init(appId, Log);
            Storage = AppManager.Storage;
            AppId = appId;
            ZoneId = AppManager.ZoneId;

            _importExportEnvironment.LinkLog(Log);
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
        public void ImportIntoDb(IList<IContentType> newTypes, IList<Entity> newEntities)
        {
            var callLog = Log.Fn($"types: {newTypes?.Count}; entities: {newEntities?.Count}", startTimer: true);
            Storage.DoWithDelayedCacheInvalidation(() =>
            {
                #region import AttributeSets if any were included but rollback transaction if necessary

                if (newTypes == null)
                    Log.A("No types to import");
                else
                    Storage.DoInTransaction(() =>
                    {
                        // get initial data state for further processing, content-typed definitions etc.
                        // important: must always create a new loader, because it will cache content-types which hurts the import
                        Storage.DoWhileQueuingVersioning(() =>
                        {
                            var logImpTypes = Log.Fn(message: "Import Types in Sys-Scope", startTimer: true);
                            // load everything, as content-type metadata is normal entities
                            // but disable initialized, as this could cause initialize stuff we're about to import
                            var appStateTemp = Storage.Loader.AppState(AppId, false); 
                            var newSetsList = newTypes.ToList();
                            // first: import the attribute sets in the system scope, as they may be needed by others...
                            // ...and would need a cache-refresh before 
                            // 2020-07-10 2dm changed to use StartsWith, as we have more scopes now
                            // before var sysAttributeSets = newSetsList.Where(a => a.Scope == Constants.ScopeSystem).ToList();
                            // warning: this may not be enough, we may have to always import the fields-scope first...
                            var sysAttributeSets = newSetsList
                                .Where(a => a.Scope?.StartsWith(Scopes.System) ?? false).ToList();
                            if (sysAttributeSets.Any())
                                MergeAndSaveContentTypes(appStateTemp, sysAttributeSets);
                            logImpTypes.Done();

                            logImpTypes = Log.Fn(message: "Import Types in non-Sys scopes", startTimer: true);
                            // now reload the app state as it has new content-types
                            // and it may need these to load the remaining attributes of the content-types
                            appStateTemp = Storage.Loader.AppState(AppId, false);

                            // now the remaining attributeSets
                            var nonSysAttribSets = newSetsList.Where(a => !sysAttributeSets.Contains(a)).ToList();
                            if (nonSysAttribSets.Any())
                                MergeAndSaveContentTypes(appStateTemp, nonSysAttribSets);
                            logImpTypes.Done();
                        });
                    });

                #endregion

                #region import Entities, but rollback transaction if necessary

                if (newEntities == null)
                    Log.A("Not entities to import");
                else
                {
                    var logImpEnts = Log.Fn(message: "Pre-Import Entities merge", startTimer: true);
                    var appStateTemp = Storage.Loader.AppState(AppId, false); // load all entities
                    newEntities = newEntities
                        .Select(entity => CreateMergedForSaving(entity, appStateTemp, SaveOptions))
                        .Where(e => e != null).ToList();

                    // HACK 2022-05-05 2dm Import Problem
                    // If we use chunks of 500, then relationships are not imported
                    // in situations where the target is in a future chunk (because the relationship can't find a matching target item)
                    // Large imports are rare in Apps, but on data-import it's common
                    // For now the hack is to allow up to 2500 in one chunk, otherwise make them smaller
                    // this is not a good final solution
                    // In general we should improve the import to 
                    var chunkSize = /*ChunkSizeAboveLimit;*/ newEntities.Count >= ChunkLimitToStartChunking ? ChunkSizeAboveLimit : ChunkLimitToStartChunking;

                    var newIEntities = newEntities.Cast<IEntity>().ToList().ChunkBy(chunkSize);
                    logImpEnts.Done();

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
                            Log.A($"Importing Chunk {cNum} #{(cNum - 1) * chunkSize + 1} - #{cNum * chunkSize}");
                            Storage.DoInTransaction(() => Storage.Save(chunk, SaveOptions));
                        })
                        //))
                        ;
                }

                #endregion
            });
            callLog.Done("done");
        }

        private void MergeAndSaveContentTypes(AppState appState, List<IContentType> contentTypes)
        {
            var callLog = Log.Fn(startTimer: true);
            // Here's the problem! #badmergeofmetadata
            contentTypes.ForEach(type => MergeContentTypeUpdateWithExisting(appState, type));
            var so = _importExportEnvironment.SaveOptions(ZoneId);// SaveOptions.Build(ZoneId);
            so.DiscardAttributesNotInType = true;
            Storage.Save(contentTypes.Cast<IContentType>().ToList(), so);
            callLog.Done("done");
        }
        
        


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

            if (existing == null)
                return callLog.Return(true, "existing not found, won't merge");

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

            return callLog.Return(true, "done");
        }

        private IEntity MergeOneMd<T>(IMetadataSource appState, int mdType, T key, IEntity newMd)
        {
            var existingMetadata = appState.GetMetadata(mdType, key, newMd.Type.NameId).FirstOrDefault();
            IEntity metadataToUse;
            if (existingMetadata == null)
            {
                metadataToUse = newMd;
                // Important to reset, otherwise the save process assumes it already exists in the DB
                metadataToUse.ResetEntityId();
                metadataToUse.SetGuid(Guid.NewGuid());
            }
            else
                metadataToUse = _entitySaver.Ready.CreateMergedForSaving(existingMetadata, newMd, SaveOptions);
            return metadataToUse;
        }


        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private Entity CreateMergedForSaving(Entity update, AppState appState, SaveOptions saveOptions)
        {
            _mergeCountToStopLogging++;
            var logDetails = _mergeCountToStopLogging <= LogMaxMerges;
            if (_mergeCountToStopLogging == LogMaxMerges)
                Log.A($"Hit {LogMaxMerges} merges, will stop logging details");
            var callLog = Log.Call<Entity>();
            #region try to get AttributeSet or otherwise cancel & log error

            var dbAttrSet = appState.GetContentType(update.Type.NameId); 

            if (dbAttrSet == null) // AttributeSet not Found
            {
                Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Error, "ContentType not found for " + update.Type.NameId));
                return callLog("error", null);
            }

            #endregion

            // Find existing Entities - meaning both draft and non-draft
            List<IEntity> existingEntities = null;
            if (update.EntityGuid != Guid.Empty)
                existingEntities = appState.List.Where(e => e.EntityGuid == update.EntityGuid).ToList();

            // set type only if is not set yet 
            if (update.Type.Id == 0)
                update.UpdateType(dbAttrSet);

            // Simplest case - nothing existing to update: return entity

            if (existingEntities == null || !existingEntities.Any())
                return callLog("is new, nothing to merge", update);

            Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Information, $"FYI: Entity {update.EntityId} already exists for guid {update.EntityGuid}"));

            // now update (main) entity id from existing - since it already exists
            var original = existingEntities.First();
            update.ResetEntityId(original.EntityId);
            var result = _entitySaver.Ready.CreateMergedForSaving(original, update, saveOptions, logDetails);
            return callLog("ok", result);
        }

        private int _mergeCountToStopLogging;
        private const int LogMaxMerges = 100;
    }
}