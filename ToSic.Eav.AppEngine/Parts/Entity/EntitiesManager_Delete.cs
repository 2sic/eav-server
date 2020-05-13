using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
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
            SystemManager.Purge(AppManager.AppId, Log);
            return ok;
        }

        internal Tuple<bool, string> CanDelete(int entityId) => AppManager.DataController.Entities.CanDeleteEntity(entityId);

        public bool Delete(Guid guid)
        {
            Log.Add($"delete guid:{guid}");
            // todo: check if GetMostCurrentDbEntity... can't be in the app-layer
            return Delete(AppManager.DataController.Entities.GetMostCurrentDbEntity(guid).EntityId);
        }

        public bool Delete(List<int> ids)
        {
            Log.Add($"delete many:{ids.Count}");
            return ids.Aggregate(true, (current, entityId) => current && Delete(entityId, null, false, true));
        }

    }
}
