﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

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

            var ok = Parent.DataController.Entities.DeleteEntity(ids, true, true);
            SystemManager.PurgeApp(Parent.AppId);
            return callLog(ok.ToString(), ok);
        }

        private Dictionary<int, Tuple<bool, string>> BatchCheckCanDelete(int[] ids, bool force, bool skipIfCant)
        {
            // Commented in v13, new implementation is based on AppState.Relationships that knows about
            // relationships with json types (that are missing in db relationships).
            //var canDeleteList = Parent.DataController.Entities.CanDeleteEntityBasedOnDbRelationships(ids);
            var canDeleteList = CanDeleteEntitiesBasedOnAppStateRelationships(ids);

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

        // Commented in v13, new implementation is based on AppState.Relationships.
        //internal Tuple<bool, string> CanDeleteEntityBasedOnDbRelationships(int entityId) 
        //    => Parent.DataController.Entities.CanDeleteEntityBasedOnDbRelationships(new[] {entityId}).First().Value;

        internal Tuple<bool, string> CanDeleteEntityBasedOnAppStateRelationships(int entityId) 
            => CanDeleteEntitiesBasedOnAppStateRelationships(new[] {entityId}).First().Value;

        private Dictionary<int, Tuple<bool, string>> CanDeleteEntitiesBasedOnAppStateRelationships(int[] ids)
        {
            var canDeleteList = new Dictionary<int, Tuple<bool, string>>();

            var relationships = Parent.Read.AppState.Relationships;
            foreach (var entityId in ids)
            {
                var messages = new List<string>();

                var parents = relationships.List.Where(r => r.Child.EntityId == entityId)
                    .Select(r => TryToGetMoreInfosAboutDependency(r.Parent)).ToList();

                if (parents.Any())
                    messages.Add(
                        $"found {parents.Count} relationships where this is a child - the parents are: {string.Join(", ", parents)}.");

                var children = relationships.List.Where(r => r.Parent.EntityId == entityId)
                    .Select(r => TryToGetMoreInfosAboutDependency(r.Child)).ToList();

                if (children.Any())
                    messages.Add(
                        $"found {children.Count} entities which are metadata for this, assigned children (like in a pipeline) or assigned for other reasons: {string.Join(", ", children)}.");

                canDeleteList.Add(entityId, Tuple.Create(!messages.Any(), string.Join(" ", messages)));
            }

            if (canDeleteList.Count != ids.Length)
                throw new Exception("Delete check failed, results doesn't match request");
            return canDeleteList;
        }

        private string TryToGetMoreInfosAboutDependency(IEntity dependency)
        {
            try
            {
                return dependency.Type.Name;
            }
            catch
            {
                return "Relationships but was not able to look up more details to show a nicer error.";
            }
        }

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
