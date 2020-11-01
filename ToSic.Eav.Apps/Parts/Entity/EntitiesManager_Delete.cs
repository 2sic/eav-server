using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public bool Delete(int id, string contentType = null, bool force = false, bool skipIfCant = false) 
            => Delete(new[] {id}, contentType, force, skipIfCant);

        /// <summary>
        /// delete an entity
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="contentType">optional content-type name to check before deleting</param>
        /// <param name="force">force delete even if there are relationships, resulting in removal of the relationships</param>
        /// <param name="skipIfCant">skip deleting if relationships exist and force is false</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Delete(int[] ids, string contentType = null, bool force = false, bool skipIfCant = false)
        {
            var callLog = Log.Call<bool>($"delete id:{ids.Length}, type:{contentType}, force:{force}", useTimer: true);

            // do optional type-check and if necessary, throw error
            BatchCheckTypesMatch(ids, contentType);

            // check if we can delete, or throw exception
            var oks = BatchCheckCanDelete(ids, force, skipIfCant);

            // var ok = ids.Aggregate(true, (current, entityId) => AppManager.DataController.Entities.DeleteEntity(entityId, false, true));
            var ok = Parent.DataController.Entities.DeleteEntity(ids, true, true);
            //AppManager.DataController.SqlDb.SaveChanges();
            SystemManager.Purge(Parent.AppId, Log);
            return callLog(ok.ToString(), ok);
        }

        private Dictionary<int, Tuple<bool, string>> BatchCheckCanDelete(int[] ids, bool force, bool skipIfCant)
        {
            var canDeleteList = Parent.DataController.Entities.CanDeleteEntity(ids);
            foreach (var canDelete in canDeleteList)
                if (!canDelete.Value.Item1 && !force && !skipIfCant)
                    throw new InvalidOperationException(
                        Log.Add($"Can't delete Item {canDelete.Key}. It is used by others: {canDelete.Value.Item2}"));
            return canDeleteList;
        }

        private void BatchCheckTypesMatch(int[] ids, string contentType)
        {
            foreach (var id in ids)
            {
                var found = Parent.Read.Entities.Get(id);
                if (contentType != null && found.Type.Name != contentType && found.Type.StaticName != contentType)
                    throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "', will not delete.");
            }
        }

        internal Tuple<bool, string> CanDelete(int entityId) => Parent.DataController.Entities.CanDeleteEntity(new[] {entityId}).First().Value;

        public bool Delete(Guid guid)
        {
            Log.Add($"delete guid:{guid}");
            // todo: check if GetMostCurrentDbEntity... can't be in the app-layer
            return Delete(Parent.DataController.Entities.GetMostCurrentDbEntity(guid).EntityId);
        }

        public bool Delete(List<int> ids)
        {
            var callLog = Log.Call<bool>($"ids:{ids.Count}", useTimer: true);
            var result = Delete(ids.ToArray(), null, false, true);
            //var result = ids.Aggregate(true, (current, entityId) => current && Delete(entityId, null, false, true));
            return callLog(result.ToString(), result);
        }

    }
}
