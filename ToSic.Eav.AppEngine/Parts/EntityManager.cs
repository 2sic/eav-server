using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public partial class EntitiesManager: ManagerBase
    {
        public EntitiesManager(AppManager app, ILog parentLog = null) : base(app, parentLog, "App.EntMan")
        {
        }


        private void PublishWithoutPurge(int entityId)
        {
            Log.Add($"PublishWithoutPurge({entityId})");

            // 1. make sure we're publishing the draft, because the entityId might be the published one...
            var contEntity = AppManager.Read.Entities.Get(entityId);
            if (contEntity == null)
                Log.Add($"Will skip, couldn't find the entity {entityId}");
            else
            {
                Log.Add($"found id: {contEntity.EntityId}, " +
                        $"rid: {contEntity.RepositoryId}, isPublished: {contEntity.IsPublished}");

                var maybeDraft = contEntity.IsPublished
                    ? contEntity.GetDraft() ?? contEntity   // if no draft exists, use current
                    : contEntity;// if it isn't published, use current

                var repoId = maybeDraft.RepositoryId;

                Log.Add($"publish requested for:{entityId}, " +
                        $"will publish: {repoId} if published false (it's: {maybeDraft.IsPublished})");

                if (!maybeDraft.IsPublished)
                    AppManager.DataController.Publishing.PublishDraftInDbEntity(repoId);
            }

            Log.Add($"/PublishWithoutPurge({entityId})");
        }

        /// <summary>
        /// Publish many entities
        /// </summary>
        public void Publish(int[] entityIds)
        {
            Log.Add(() => "Publish(" + entityIds.Length + " items [" + string.Join(",", entityIds) + "])");
            foreach (var eid in entityIds)
                try
                {
                    PublishWithoutPurge(eid);
                }
                catch (Repository.Efc.Parts.EntityAlreadyPublishedException) { /* ignore */ }
            // for now, do a full purge as it's the safest. in the future, maybe do a partial cache-update
            SystemManager.Purge(AppManager.AppId);
            Log.Add("/Publish(...)");
        }

        /// <summary>
        /// Publish a single entity 
        /// </summary>
        public void Publish(int entityId) => Publish(new[] { entityId });


        #region Delete

        /// <summary>
        /// delete an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contentType">optional content-type name to check before deleting</param>
        /// <param name="force">force delete even if there are relationships, resulting in removal of the relationships</param>
        /// <param name="skipIfCant">skip deleting if relationships exist and force is false</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Delete(int id, string contentType = null, bool force = false, bool skipIfCant = false)
        {
            Log.Add("delete id:" + id + ", type:" + contentType + ", force:" + force);

            #region do optional type-check and if necessary, throw error
            var found = AppManager.Read.Entities.Get(id);
            if (contentType != null && found.Type.Name != contentType && found.Type.StaticName != contentType)
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "', will not delete.");
            #endregion

            #region check if we can delete, or throw exception

            var canDelete = CanDelete(id);
            if (!canDelete.Item1 && !force && !skipIfCant)
                throw new InvalidOperationException($"Item {id} cannot be deleted. It is used by other items: {canDelete.Item2}");
            #endregion

            var ok = AppManager.DataController.Entities.DeleteEntity(id, true, true);
            SystemManager.Purge(AppManager.AppId);
            return ok;
        }

        internal Tuple<bool, string> CanDelete(int entityId) => AppManager.DataController.Entities.CanDeleteEntity(entityId);

        public bool Delete(Guid guid)
        {
            Log.Add($"delete guid:{guid}");
            // todo: check if getmostcurrentdb... can't be in the app-layer
            return Delete(AppManager.DataController.Entities.GetMostCurrentDbEntity(guid).EntityId);
        }

        public bool Delete(List<int> ids)
        {
            Log.Add($"delete many:{ids.Count}");
            return ids.Aggregate(true, (current, entityId) => current && Delete(entityId, null, false, true));
        }

        #endregion


        public int Save(IEntity entity, SaveOptions saveOptions = null) 
            => Save(new List<IEntity> {entity}, saveOptions).FirstOrDefault();

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions = null)
        {
            var wrapLog = Log.Call("", message: "save count:" + entities.Count + ", with Options:" + (saveOptions != null));
            saveOptions = saveOptions ?? SaveOptions.Build(AppManager.ZoneId);

            // ensure the type-definitions are real, not just placeholders
            foreach (var entity in entities)
                if (entity is Entity e2
                    && !e2.Type.IsDynamic // it's not dynamic
                    && e2.Type.Attributes == null) // it doesn't have attributes, so it must have been in-memory
                {
                    var newtype = AppManager.Read.ContentTypes.Get(entity.Type.Name);
                    if(newtype != null) e2.UpdateType(newtype); // try to update, but leave if not found
                }

            // attach relationship resolver - important when saving data which doesn't yet have the guid
            entities.ForEach(AppManager.AppState.Relationships.AttachRelationshipResolver);

            List<int> ids = null;
            AppManager.DataController.DoButSkipAppCachePurge(() =>
                AppManager.DataController.DoWhileQueueingRelationships(() =>
                {
                    ids = AppManager.DataController.Save(entities, saveOptions);
                })
            );

            // Tell the cache to do a partial update
            /*Factory.GetAppsCache*/
            Eav.Apps.Apps.Cache.Update(AppManager, ids, Log);

            //AppManager.DataController.Loader.Update(AppManager.Package, 
            //    AppStateLoadSequence.ItemLoad, ids.ToArray(), Log);
            //// clear cache of this app
            //new SystemManager(Log).InformOfPartialUpdate(AppManager, ids);
            wrapLog($"ids:{ids.Count}");
            return ids;
        }

        public Tuple<int, Guid> Create(string typeName, Dictionary<string, object> values, ITarget metadataFor = null)
        {
            var wrapLog = Log.Call($"type:{typeName}, val-count:{values.Count}, meta:{metadataFor}");
            var newEnt = new Entity(AppManager.AppId, Guid.NewGuid(), AppManager.Read.ContentTypes.Get(typeName), values);
            if (metadataFor != null) newEnt.SetMetadata(metadataFor as Metadata.Target);
            var eid = Save(newEnt);
            var guid = AppManager.DataController.Entities.TempLastSaveGuid;
            wrapLog($"id:{eid}, guid:{guid}");
            return new Tuple<int, Guid>(eid, guid);
        }

        public void SaveMetadata(Metadata.Target target, string typeName, Dictionary<string, object> values)
        {
            var wrapLog = Log.Call("target:" + target.KeyNumber + "/" + target.KeyGuid + ", values count:" + values.Count);

            if (target.TargetType != Constants.MetadataForAttribute || target.KeyNumber == null || target.KeyNumber == 0)
                throw new NotImplementedException("atm this command only creates metadata for entities with id-keys");

            // see if a metadata already exists which we would update
            var existingEntity = AppManager./*Cache*/AppState.List
                .FirstOrDefault(e => e.MetadataFor?.TargetType == target.TargetType && e.MetadataFor?.KeyNumber == target.KeyNumber);
            if (existingEntity != null)
                UpdateParts(existingEntity.EntityId, values);
            else
            {
                var saveEnt = new Entity(AppManager.AppId, Guid.NewGuid(), AppManager.Read.ContentTypes.Get(typeName), values);
                saveEnt.SetMetadata(target);
                Save(saveEnt);
            }
            wrapLog("ok");
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        public void UpdateParts(int id, Dictionary<string, object> values)
        {
            var wrapLog = Log.Call();
            var saveOptions = SaveOptions.Build(AppManager.ZoneId);
            saveOptions.PreserveUntouchedAttributes = true;
            saveOptions.PreserveUnknownLanguages = true;

            var orig = IEntityExtensions.FindRepoId(AppManager./*Cache*/AppState.List,id);
            var tempEnt = new Entity(AppManager.AppId, 0, orig.Type, values);
            var saveEnt = new EntitySaver(Log).CreateMergedForSaving(orig, tempEnt, saveOptions);
            Save(saveEnt, saveOptions);
            wrapLog(null);
        }

        /// <summary>
        /// Get an entity, or create it with the values provided.
        /// Important for use cases, where an information must exist for sure, so it would be created with the provided defaults
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="typeName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetOrCreate(Guid newGuid, string typeName, Dictionary<string, object> values)
        {
            Log.Add($"get or create guid:{newGuid}, type:{typeName}, val-count:{values.Count}");
            if (AppManager.DataController.Entities.EntityExists(newGuid))
            {
                // check if it's deleted - if yes, resurrect
                var existingEnt = AppManager.DataController.Entities.GetEntitiesByGuid(newGuid).First();
                if (existingEnt.ChangeLogDeleted != null)
                    existingEnt.ChangeLogDeleted = null;

                return existingEnt.EntityId;
            }

            var newE = new Entity(AppManager.AppId, newGuid, AppManager.Read.ContentTypes.Get(typeName), values);
            return Save(newE);
        }


        #region Helpers to get things done
        // todo: probably should move to the new Eav.Apps section, but for that we must
        public void ModifyItemList(int parentId, string field, IItemListAction actionToPerform)
        {
            Log.Add($"modify item list parent:{parentId}, field:{field}, action:{actionToPerform}");
            var parentEntity = AppManager.Read.Entities.Get(parentId);
            var parentField = parentEntity.GetBestValue(field);

            if (!(parentField is IEnumerable<IEntity> fieldList))
                throw new Exception("field " + field + " doesn't seem to be a list of content-items, must abort");

            var ids = actionToPerform.Change(fieldList.ToList());
            if (ids == null) return;

            // save
            var values = new Dictionary<string, object> { { field, ids.Select(e => e?.EntityGuid).ToList() } };
            AppManager.Entities.UpdateParts(parentEntity.EntityId, values);
        }
        #endregion


        public ExportListXml Exporter(IContentType contentType) 
            => new ExportListXml(AppManager.AppState, contentType, Log);
        public ExportListXml Exporter(string contentType) 
            => new ExportListXml(AppManager.AppState, AppManager.Read.ContentTypes.Get(contentType), Log);

        public ImportListXml Importer(
            string contentTypeName,
            Stream dataStream,
            IEnumerable<string> languages,
            string documentLanguageFallback,
            ImportDeleteUnmentionedItems deleteSetting,
            ImportResourceReferenceMode resolveReferenceMode)
        {
            var ct = AppManager.Read.ContentTypes.Get(contentTypeName);
            return new ImportListXml(AppManager, ct, 
                dataStream, languages, documentLanguageFallback, 
                deleteSetting, resolveReferenceMode, Log);
        }
    }
}
