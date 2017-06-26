using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Versioning;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public partial class EntitiesManager: BaseManager
    {
        public EntitiesManager(AppManager app) : base(app)
        {
        }

        /// <summary>
        /// Publish an entity 
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool Publish(int repositoryId, bool state)
        {
            _appManager.DataController.Publishing.PublishDraftInDbEntity(repositoryId);//, state);
            return state;
        }

        #region Delete

        /// <summary>
        /// delete an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Delete(int id, bool force = false)
        {
            var canDelete = _appManager.DataController.Entities.CanDeleteEntity(id);
            if (!canDelete.Item1 && !force)
                throw new Exception(canDelete.Item2);
            return _appManager.DataController.Entities.DeleteEntity(id);
        }

        public bool Delete(Guid guid) => _appManager.DataController.Entities.DeleteEntity(guid);

        public bool DeletePossible(int entityId) => _appManager.DataController.Entities.CanDeleteEntity(entityId).Item1;

        public string DeleteHinderance(int entityId) => _appManager.DataController.Entities.CanDeleteEntity(entityId).Item2;
        #endregion

        public int Save(IEntity entity, SaveOptions saveOptions = null) => Save(new List<IEntity> {entity}, saveOptions).FirstOrDefault();

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions = null)
        {
            saveOptions = saveOptions ?? SaveOptions.Build(_appManager.ZoneId);
            saveOptions.DelayRelationshipSave = true; // save all relationships in one round when ready...

            var ids = _appManager.DataController.Entities.SaveEntity(entities, saveOptions);

            // clear cache of this app
            SystemManager.Purge(_appManager.AppId);
            return ids;
        }


        public Tuple<int, Guid> Create(string typeName, Dictionary<string, object> values, IIsMetadata isMetadata = null)
        {
            var newEnt = new Entity(0, typeName, values);
            if (isMetadata != null) newEnt.SetMetadata(isMetadata as Metadata);
            var eid = Save(newEnt);

            return new Tuple<int, Guid>(eid, _appManager.DataController.Entities.TempLastSaveGuid);
        }

        public void TempAddMetadata(Metadata target, string typeName, Dictionary<string, object> values)
        {
            if(target.TargetType != Constants.MetadataForField || target.KeyNumber == null || target.KeyNumber == 0)
                throw new NotImplementedException("atm this command only creates metadata for entities with id-keys");

            // see if a metadata already exists which we would update
            var existingEntity = _appManager.Cache.LightList.FirstOrDefault(e => e.Metadata?.TargetType == target.TargetType && e.Metadata?.KeyNumber == target.KeyNumber);
            if (existingEntity != null)
                UpdateParts(existingEntity.EntityId, values);
            else
            {
                var saveEnt = new Entity(0, typeName, values);
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
            //saveOptions.Languages = _appManager.Read.Zone.Languages(true);

            var orig = _appManager.Cache.List[id];
            var tempEnt = new Entity(0, "", values);
            var saveEnt = EntitySaver.CreateMergedForSaving(orig, tempEnt, saveOptions);
            Save(saveEnt, saveOptions);

            //_appManager.DataController.Entities.UpdateAttributesAndPublishing(id, values); //, dimensionIds: dimensionIds);
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
            if (_appManager.DataController.Entities.EntityExists(newGuid))
            {
                // check if it's deleted - if yes, resurrect
                var existingEnt = _appManager.DataController.Entities.GetEntitiesByGuid(newGuid).First();
                if (existingEnt.ChangeLogDeleted != null)
                    existingEnt.ChangeLogDeleted = null;

                return existingEnt.EntityId;
            }

            var newE = new Entity(newGuid, contentTypeName, values);
            return Save(newE);
        }

    }
}
