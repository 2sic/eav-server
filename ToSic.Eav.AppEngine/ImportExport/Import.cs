using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
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
        #region Private Fields
        //private readonly DbDataController _dbDeepAccess;
        private AppState _entireApp;
        
        internal AppManager App;
        internal IStorage Storage;
        public SaveOptions SaveOptions;

        private readonly int AppId;
        private readonly int ZoneId;

        #endregion

        /// <summary>
        /// Initializes a new instance of the Import class.
        /// </summary>
        public Import(int? zoneId, int appId, bool skipExistingAttributes = true, bool preserveUntouchedAttributes = true, ILog parentLog = null): base("Eav.Import", parentLog, "constructor")
        {
            App = zoneId.HasValue ? new AppManager(zoneId.Value, appId) : new AppManager(appId, Log);
            Storage = App.Storage;

            // now save the resolved zone/app IDs
            AppId = appId;
            ZoneId = App.ZoneId;
            var iex = Factory.Resolve<IImportExportEnvironment>();
            iex.LinkLog(Log);
            SaveOptions = iex.SaveOptions(ZoneId);

            SaveOptions.SkipExistingAttributes = skipExistingAttributes;

            SaveOptions.PreserveUntouchedAttributes = preserveUntouchedAttributes;
        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        public void ImportIntoDb(IEnumerable<ContentType> newAttributeSets, IEnumerable<Entity> newEntities)
        {
            Storage.DoWithDelayedCacheInvalidation(() =>
            {
                // run import, but rollback transaction if necessary
                Storage.DoInTransaction(() =>
                {
                    // get initial data state for further processing, content-typed definitions etc.
                    // important: must always create a new loader, because it will cache content-types which hurts the import
                    #region import AttributeSets if any were included

                    if (newAttributeSets != null)
                        Storage.DoWhileQueuingVersioning(() =>
                        {
                            _entireApp = Storage.Loader.AppState(AppId, parentLog:Log); // load everything, as content-type metadata is normal entities
                            var newSetsList = newAttributeSets.ToList();
                            // first: import the attribute sets in the system scope, as they may be needed by others...
                            // ...and would need a cache-refresh before 
                            var sysAttributeSets = newSetsList.Where(a => a.Scope == Constants.ScopeSystem).ToList();
                            if (sysAttributeSets.Any())
                                MergeAndSaveContentTypes(sysAttributeSets);

                            _entireApp = Storage.Loader.AppState(AppId, parentLog: Log); // load everything, as content-type metadata is normal entities

                            // now the remaining attributeSets
                            var nonSysAttribSets = newSetsList.Where(a => !sysAttributeSets.Contains(a)).ToList();
                            if (nonSysAttribSets.Any())
                                MergeAndSaveContentTypes(nonSysAttribSets);
                        });

                    #endregion

                    #region import Entities

                    if (newEntities != null)
                    {
                        _entireApp = Storage.Loader.AppState(AppId, parentLog: Log); // load all entities
                        newEntities = newEntities
                            .Select(entity => CreateMergedForSaving(entity, _entireApp, SaveOptions))
                            .Where(e => e != null).ToList();
                        Storage.Save(newEntities.Cast<IEntity>().ToList(), SaveOptions);
                    }

                    #endregion

                });
            });
        }

        private void MergeAndSaveContentTypes(List<ContentType> contentTypes)
        {
            contentTypes.ForEach(MergeContentTypeUpdateWithExisting);
            var so = SaveOptions.Build(ZoneId);
            so.DiscardattributesNotInType = true;
            Storage.Save(contentTypes.Cast<IContentType>().ToList(), so);
        }
        
        


        private void MergeContentTypeUpdateWithExisting(IContentType contentType)
        {
            var existing = _entireApp.GetContentType(contentType.StaticName);
            if (existing == null) return;

            foreach (var newAttrib in contentType.Attributes)
            {
                var existAttrib = existing.Attributes.FirstOrDefault(a => a.Name == newAttrib.Name);
                if (existAttrib == null) continue;

                var impMeta = ((ContentTypeAttribute) newAttrib).Metadata;
                var newMetaList = new List<IEntity>();
                foreach (var newMd in impMeta)
                {
                    var existingMetadata = _entireApp.Get(Constants.MetadataForAttribute, existAttrib.AttributeId, newMd.Type.StaticName).FirstOrDefault();
                    if (existingMetadata == null)
                        newMetaList.Add(newMd);
                    else
                        newMetaList.Add(new EntitySaver(Log).CreateMergedForSaving(existingMetadata, newMd, SaveOptions) as Entity);
                }
                ((ContentTypeAttribute) newAttrib).Metadata.Use(newMetaList);
            }
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private Entity CreateMergedForSaving(Entity update, AppState appState, SaveOptions saveOptions)
        {
           #region try to get AttributeSet or otherwise cancel & log error

            var dbAttrSet = appState.GetContentType(update.Type.StaticName); 
            // .ContentTypes.Values.FirstOrDefault(ct => String.Equals(ct.StaticName, update.Type.StaticName, StringComparison.InvariantCultureIgnoreCase));

            if (dbAttrSet == null) // AttributeSet not Found
            {
                Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Error, "ContentType not found for " + update.Type.StaticName));
                return null;
            }

            #endregion

            // Find existing Enties - meaning both draft and non-draft
            List<IEntity> existingEntities = null;
            if (update.EntityGuid != Guid.Empty)
                existingEntities = appState.List.Where(e => e.EntityGuid == update.EntityGuid).ToList();

            #region Simplest case - nothing existing to update: return entity

            if (existingEntities == null || !existingEntities.Any())
                return update;

            #endregion

            Storage.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Information, $"FYI: Entity {update.EntityId} already exists for guid {update.EntityGuid}"));

            // now update (main) entity id from existing - since it already exists
            var original = existingEntities.First();
            update.ResetEntityId(original.EntityId);
            return new EntitySaver(Log).CreateMergedForSaving(original, update, saveOptions) as Entity;

        }
    }
}