using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Versioning;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class EntitiesManager: BaseManager
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

        public void Save(IEntity entity, SaveOptions saveOptions = null) => Save(new List<IEntity> {entity}, saveOptions);

        public void Save(List<IEntity> entities, SaveOptions saveOptions = null)
        {
            var env = Factory.Resolve<IImportExportEnvironment>();

            saveOptions = saveOptions ?? new SaveOptions();
            saveOptions.PrimaryLanguage = saveOptions.PrimaryLanguage ?? env.DefaultLanguage;
            //foreach (var entity in entities)
            //    SetPublishDraftState(entity, saveOptions);

            foreach (var entity in entities)
                _appManager.DataController.Entities.SaveEntity(entity, saveOptions);
            _appManager.DataController.Relationships.ImportRelationshipQueueAndSave();
            
            // clear cache of this app
            SystemManager.Purge(_appManager.AppId);
        }

        //private void SetPublishDraftState(IEntity entity, SaveOptions so)
        //{
        //    // no guid to use to check / change publication state
        //    if (entity.EntityGuid == Guid.Empty) return;

        //    // see if there is anything existing with this guid (in this app...)
        //    var dbEnts = _appManager.Cache.LightList.Where(e => e.EntityGuid == entity.EntityGuid).ToList();
        //    if (!dbEnts.Any()) return;

        //    // if new isn't published, existing is published and we can branch, then specify that
        //    if (!entity.IsPublished && so.AllowBranching && dbEnts.Count(e => e.IsPublished == false) == 0)// !((Entity)entity).OnSaveForceNoBranching)
        //    {
        //        var publishedId = dbEnts.First().EntityId; // any one has the right id
        //        ((Entity)entity).SetPublishedIdForSaving(publishedId);
        //        return;
        //    }
        //}


        public Tuple<int, Guid> Create(string typeName, Dictionary<string, object> values, IIsMetadata isMetadata = null)
        {
            //var contentType = _appManager.Cache.GetContentType(typeName);
            //var ent = _appManager.DataController.Entities.AddEntity(contentType.ContentTypeId, values, isMetadata);
            //return new Tuple<int, Guid>(ent.EntityId, ent.EntityGuid);
            var newEnt = new Entity(0, typeName, values);
            if(isMetadata != null )newEnt.SetMetadata(isMetadata as Metadata);
            var eid = _appManager.DataController.Entities.SaveEntity(newEnt, new SaveOptions());

            return new Tuple<int, Guid>(eid, _appManager.DataController.Entities.TempLastSaveGuid);
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        /// <param name="dimensionIds"></param>
        public void Update(int id, Dictionary<string, object> values, ICollection<int> dimensionIds = null)
            => _appManager.DataController.Entities.UpdateAttributesAndPublishing(id, values, dimensionIds: dimensionIds);

        /// <summary>
        /// Get an entity, or create it with the values provided.
        /// Important for use cases, where an information must exist for sure, so it would be created with the provided defaults
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="contentTypeName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetOrCreate(Guid? newGuid, string contentTypeName, Dictionary<string, object> values)
        {
            if (newGuid.HasValue && _appManager.DataController.Entities.EntityExists(newGuid.Value))
            {
                // check if it's deleted - if yes, resurrect
                var existingEnt = _appManager.DataController.Entities.GetEntitiesByGuid(newGuid.Value).First();
                if (existingEnt.ChangeLogDeleted != null)
                    existingEnt.ChangeLogDeleted = null;

                return existingEnt.EntityId;
            }
            //var contentType = _appManager.Cache.GetContentType(contentTypeName).ContentTypeId;
            //return _appManager.DataController.Entities.AddEntity(contentType, values, entityGuid: newGuid).EntityId;

            var newE = new Entity(newGuid.Value, contentTypeName, values);
            return _appManager.DataController.Entities.SaveEntity(newE, new SaveOptions());
        }


        public List<ItemHistory> GetHistory(int id) => _appManager.DataController.Versioning.GetHistoryList(id, true);

        public void RestorePrevious(int id, int historyId)
            => _appManager.DataController.Versioning.RestoreEntity(id, historyId); 
    }
}
