using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Apps.Manage
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class EntitiesManager
    {
        private readonly AppManager _appManager;
        internal EntitiesManager(AppManager appManager)
        {
            _appManager = appManager;
        }


        /// <summary>
        /// Publish an entity 
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool Publish(int repositoryId, bool state)
        {
            _appManager.DataController.Publishing.PublishDraftInDbEntity(repositoryId, state);
            return state;
        }

        #region Delete
        /// <summary>
        /// delete an entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Delete(int id)
        {
            var canDelete = _appManager.DataController.Entities.CanDeleteEntity(id);
            if (!canDelete.Item1)
                throw new Exception(canDelete.Item2);
            return _appManager.DataController.Entities.DeleteEntity(id);
        }

        public bool DeletePossible(int entityId) => _appManager.DataController.Entities.CanDeleteEntity(entityId).Item1;

        public string DeleteHinderance(int entityId) => _appManager.DataController.Entities.CanDeleteEntity(entityId).Item2;
        #endregion


        /// <summary>
        /// Create an entity in this app, optionally with a GUID provided for the new ID
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="values"></param>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        public Tuple<int, Guid> Create(string typeName, Dictionary<string, object> values, Guid? entityGuid = null)
        {
            var contentType = _appManager.Cache.GetContentType(typeName);
            var ent = _appManager.DataController.Entities.AddEntity(contentType.AttributeSetId, values, null, null, entityGuid: entityGuid);
            return new Tuple<int, Guid>(ent.EntityID, ent.EntityGUID);
        }


        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        public void Update(int id, Dictionary<string, object> values)
            => _appManager.DataController.Entities.UpdateEntity(id, values);

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

                return existingEnt.EntityID;
            }

            return Create(contentTypeName, values, newGuid).Item1;
        }

    }
}
