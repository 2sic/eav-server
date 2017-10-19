using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public partial class EntitiesManager: ManagerBase
    {
        public EntitiesManager(AppManager app, Log parentLog = null) : base(app, parentLog, "App.EntMan")
        {
        }

        /// <summary>
        /// Publish an entity 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool Publish(int entityId, bool state)
        {
            Log.Add("publish id:" + entityId + ", state:" + state);
            _appManager.DataController.Publishing.PublishDraftInDbEntity(entityId); 
            SystemManager.Purge(_appManager.AppId);
            return state;
        }

        /// <summary>
        /// Publish an entity 
        /// </summary>
        /// <returns></returns>
        public void Publish(int[] entityIds)
        {
            Log.Add(() => "publish many:" + entityIds.Length + " items [" + string.Join(",", entityIds) + "]");
            foreach (var eid in entityIds)
            {
                try
                {
                    Log.Add("publish id:" + eid);
                    _appManager.DataController.Publishing.PublishDraftInDbEntity(eid);
                }
                catch (Repository.Efc.Parts.EntityAlreadyPublishedException) { }
            }
            SystemManager.Purge(_appManager.AppId);
        }

        #region Delete

        /// <summary>
        /// delete an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contentType">optional content-type name to check before deleting</param>
        /// <param name="force"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Delete(int id, string contentType = null, bool force = false)
        {
            Log.Add("delete id:" + id + ", type:" + contentType + ", force:" + force);

            #region do optional type-check and if necessary, throw error
            var found = _appManager.Read.Entities.Get(id);
            if (contentType != null && found.Type.Name != contentType && found.Type.StaticName != contentType)
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "', will not delete.");
            #endregion

            #region check if we can delete, or throw exception
            var canDelete = _appManager.DataController.Entities.CanDeleteEntity(id);
            if (!canDelete.Item1 && !force)
                throw new InvalidOperationException($"Item {id} cannot be deleted. It is used by other items: {canDelete.Item2}");
            #endregion

            var ok = _appManager.DataController.Entities.DeleteEntity(id, true, true);
            SystemManager.Purge(_appManager.AppId);
            return ok;
        }

        public bool Delete(Guid guid)
        {
            Log.Add($"delete guid:{guid}");
            return _appManager.DataController.Entities.DeleteEntity(_appManager.DataController.Entities
                .GetMostCurrentDbEntity(guid).EntityId);
        }

        #endregion
        
        public int Save(IEntity entity, SaveOptions saveOptions = null) => Save(new List<IEntity> {entity}, saveOptions).FirstOrDefault();

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions = null)
        {
            Log.Add("save count:" + entities.Count + ", with Options:" + (saveOptions != null));
            saveOptions = saveOptions ?? SaveOptions.Build(_appManager.ZoneId);
            //saveOptions.DelayRelationshipSave = true; // save all relationships in one round when ready...
            List<int> ids = null;
            _appManager.DataController.DoWhileQueueingRelationships(() =>
            {
                ids = _appManager.DataController.Entities.SaveEntity(entities, saveOptions);
            });
            // clear cache of this app
            SystemManager.Purge(_appManager.AppId);
            return ids;
        }

        public Tuple<int, Guid> Create(string typeName, Dictionary<string, object> values, IIsMetadata isMetadata = null)
        {
            Log.Add($"create type:{typeName}, meta:{isMetadata}, val-count:{values.Count}");
            var newEnt = new Entity(_appManager.AppId, 0, typeName, values);
            if (isMetadata != null) newEnt.SetMetadata(isMetadata as Metadata);
            var eid = Save(newEnt);

            return new Tuple<int, Guid>(eid, _appManager.DataController.Entities.TempLastSaveGuid);
        }

        public void SaveMetadata(Metadata target, string typeName, Dictionary<string, object> values)
        {
            Log.Add("save metadata target:" + target.KeyNumber + "/" + target.KeyGuid + ", values count:" + values.Count);

            if (target.TargetType != Constants.MetadataForAttribute || target.KeyNumber == null || target.KeyNumber == 0)
                throw new NotImplementedException("atm this command only creates metadata for entities with id-keys");

            // see if a metadata already exists which we would update
            var existingEntity = _appManager.Cache.LightList.FirstOrDefault(e => e.Metadata?.TargetType == target.TargetType && e.Metadata?.KeyNumber == target.KeyNumber);
            if (existingEntity != null)
                UpdateParts(existingEntity.EntityId, values);
            else
            {
                var saveEnt = new Entity(_appManager.AppId, 0, typeName, values);
                saveEnt.SetMetadata(target);
                Save(saveEnt);
            }
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        public void UpdateParts(int id, Dictionary<string, object> values)
        {
            var saveOptions = SaveOptions.Build(_appManager.ZoneId);
            saveOptions.PreserveUntouchedAttributes = true;
            saveOptions.PreserveUnknownLanguages = true;

            var orig = _appManager.Cache.List[id];
            var tempEnt = new Entity(_appManager.AppId, 0, "", values);
            var saveEnt = new EntitySaver(Log).CreateMergedForSaving(orig, tempEnt, saveOptions);
            Save(saveEnt, saveOptions);
        }

        /// <summary>
        /// Get an entity, or create it with the values provided.
        /// Important for use cases, where an information must exist for sure, so it would be created with the provided defaults
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="contentTypeName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetOrCreate(Guid newGuid, string contentTypeName, Dictionary<string, object> values)
        {
            Log.Add($"get or create guid:{newGuid}, type:{contentTypeName}, val-count:{values.Count}");
            if (_appManager.DataController.Entities.EntityExists(newGuid))
            {
                // check if it's deleted - if yes, resurrect
                var existingEnt = _appManager.DataController.Entities.GetEntitiesByGuid(newGuid).First();
                if (existingEnt.ChangeLogDeleted != null)
                    existingEnt.ChangeLogDeleted = null;

                return existingEnt.EntityId;
            }

            var newE = new Entity(_appManager.AppId, newGuid, contentTypeName, values);
            return Save(newE);
        }


        public ExportListXml Exporter(IContentType contentType) 
            => new ExportListXml(_appManager.Cache.AppDataPackage, contentType, Log);
        public ExportListXml Exporter(string contentType) 
            => new ExportListXml(_appManager.Cache.AppDataPackage, _appManager.Read.ContentTypes.Get(contentType), Log);
    }
}
