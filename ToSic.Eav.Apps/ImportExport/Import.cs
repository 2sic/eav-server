﻿using System;
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
using Entity = ToSic.Eav.Data.Entity;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import: HasLog
    {
        private const int ChunkSize = 500;

        #region Constructor / DI

        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly IImportExportEnvironment _importExportEnvironment;
        internal AppManager AppManager;

        public Import(Lazy<AppManager> appManagerLazy, IImportExportEnvironment importExportEnvironment) : base("Eav.Import")
        {
            _appManagerLazy = appManagerLazy;
            _importExportEnvironment = importExportEnvironment;
        }

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
        public void ImportIntoDb(IList<ContentType> newTypes, IList<Entity> newEntities)
        {
            var callLog = Log.Call($"types: {newTypes?.Count}; entities: {newEntities?.Count}", useTimer: true);
            Storage.DoWithDelayedCacheInvalidation(() =>
            {
                #region import AttributeSets if any were included but rollback transaction if necessary

                if (newTypes == null)
                    Log.Add("No types to import");
                else
                    Storage.DoInTransaction(() =>
                    {
                        // get initial data state for further processing, content-typed definitions etc.
                        // important: must always create a new loader, because it will cache content-types which hurts the import
                        Storage.DoWhileQueuingVersioning(() =>
                        {
                            var logImpTypes = Log.Call(message: "Import Types in Sys-Scope", useTimer: true);
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
                                .Where(a => a.Scope?.StartsWith(Constants.ScopeSystem) ?? false).ToList();
                            if (sysAttributeSets.Any())
                                MergeAndSaveContentTypes(appStateTemp, sysAttributeSets);
                            logImpTypes(null);

                            logImpTypes = Log.Call(message: "Import Types in non-Sys scopes", useTimer: true);
                            // now reload the app state as it has new content-types
                            // and it may need these to load the remaining attributes of the content-types
                            appStateTemp = Storage.Loader.AppState(AppId, false);

                            // now the remaining attributeSets
                            var nonSysAttribSets = newSetsList.Where(a => !sysAttributeSets.Contains(a)).ToList();
                            if (nonSysAttribSets.Any())
                                MergeAndSaveContentTypes(appStateTemp, nonSysAttribSets);
                            logImpTypes(null);
                        });
                    });

                #endregion

                #region import Entities, but rollback transaction if necessary

                if (newEntities == null)
                    Log.Add("Not entities to import");
                else
                {
                    var logImpEnts = Log.Call(message: "Pre-Import Entities merge", useTimer: true);
                    var appStateTemp = Storage.Loader.AppState(AppId, false); // load all entities
                    newEntities = newEntities
                        .Select(entity => CreateMergedForSaving(entity, appStateTemp, SaveOptions))
                        .Where(e => e != null).ToList();
                    var newIEntities = newEntities.Cast<IEntity>().ToList().ChunkBy(ChunkSize);
                    logImpEnts(null);

                    // Import in chunks
                    var cNum = 0;
                    newIEntities.ForEach(chunk =>
                    {
                        cNum++;
                        Log.Add($"Importing Chunk {cNum} #{(cNum - 1) * ChunkSize + 1} - #{cNum * ChunkSize}");
                        Storage.DoInTransaction(() => Storage.Save(chunk, SaveOptions));
                    }); 
                }

                #endregion
            });
            callLog("done");
        }

        private void MergeAndSaveContentTypes(AppState appState, List<ContentType> contentTypes)
        {
            var callLog = Log.Call(useTimer: true);
            contentTypes.ForEach(type => MergeContentTypeUpdateWithExisting(appState, type));
            var so = SaveOptions.Build(ZoneId);
            so.DiscardattributesNotInType = true;
            Storage.Save(contentTypes.Cast<IContentType>().ToList(), so);
            callLog("done");
        }
        
        


        private bool MergeContentTypeUpdateWithExisting(AppState appState, IContentType contentType)
        {
            var callLog = Log.Call<bool>();
            var existing = appState.GetContentType(contentType.StaticName);

            Log.Add("New CT, must reset attributes");
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
                return callLog("existing not found, won't merge", true);

            Log.Add("found existing, will merge");
            foreach (var newAttribute in contentType.Attributes)
            {
                var oldAttr = existing.Attributes.FirstOrDefault(a => a.Name == newAttribute.Name);
                if (oldAttr == null)
                {
                    Log.Add($"New attr {newAttribute.Name} not found on original, merge not needed");
                    continue;
                }

                var newMetaList = newAttribute.Metadata
                    .Select(impMd => MergeOneMd(appState, Constants.MetadataForAttribute, oldAttr.AttributeId, impMd))
                    .ToList();

                if(newAttribute.Metadata.Permissions.Any())
                    newMetaList.AddRange(newAttribute.Metadata.Permissions.Select(p => p.Entity));

                newAttribute.Metadata.Use(newMetaList);
            }

            // check if the content-type has metadata, which needs merging
            var merged = contentType.Metadata
                .Select(impMd => MergeOneMd(appState, Constants.MetadataForContentType, contentType.StaticName, impMd))
                .ToList();
            merged.AddRange(contentType.Metadata.Permissions.Select(p => p.Entity));
            contentType.Metadata.Use(merged);

            return callLog("done", true);
        }

        private IEntity MergeOneMd<T>(IMetadataSource appState, int mdType, T key, IEntity newMd)
        {
            var existingMetadata = appState.GetMetadata(mdType, key, newMd.Type.StaticName).FirstOrDefault();
            var metadataToUse = existingMetadata == null
                ? newMd
                : new EntitySaver(Log).CreateMergedForSaving(existingMetadata, newMd, SaveOptions);
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
                Log.Add($"Hit {LogMaxMerges} merges, will stop logging details");
            var callLog = Log.Call<Entity>();
            #region try to get AttributeSet or otherwise cancel & log error

            var dbAttrSet = appState.GetContentType(update.Type.StaticName); 

            if (dbAttrSet == null) // AttributeSet not Found
            {
                Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Error, "ContentType not found for " + update.Type.StaticName));
                return callLog("error", null);
            }

            #endregion

            // Find existing Entities - meaning both draft and non-draft
            List<IEntity> existingEntities = null;
            if (update.EntityGuid != Guid.Empty)
                existingEntities = appState.List.Where(e => e.EntityGuid == update.EntityGuid).ToList();

            // Simplest case - nothing existing to update: return entity

            if (existingEntities == null || !existingEntities.Any())
                return callLog("is new, nothing to merge", update);

            Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Information, $"FYI: Entity {update.EntityId} already exists for guid {update.EntityGuid}"));

            // now update (main) entity id from existing - since it already exists
            var original = existingEntities.First();
            update.ResetEntityId(original.EntityId);
            var result = new EntitySaver(Log).CreateMergedForSaving(original, update, saveOptions, logDetails);
            return callLog("ok", result);
        }

        private int _mergeCountToStopLogging;
        private const int LogMaxMerges = 100;
    }
}