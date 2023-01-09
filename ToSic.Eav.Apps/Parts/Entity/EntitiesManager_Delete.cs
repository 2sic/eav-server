using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public bool Delete(int id, string contentType = null, bool force = false, bool skipIfCant = false, int? parentId = null, string parentField = null) 
            => Delete(new[] {id}, contentType, force, skipIfCant, parentId, parentField);

        /// <summary>
        /// delete an entity
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="contentType">optional content-type name to check before deleting</param>
        /// <param name="force">force delete even if there are relationships, resulting in removal of the relationships</param>
        /// <param name="skipIfCant">skip deleting if relationships exist and force is false</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Delete(int[] ids, string contentType = null, bool force = false, bool skipIfCant = false, int? parentId = null, string parentField = null)
        {
            var callLog = Log.Fn<bool>($"delete id:{ids.Length}, type:{contentType}, force:{force}", timer: true);

            // do optional type-check and if necessary, throw error
            BatchCheckTypesMatch(ids, contentType);

            // get related metadata ids
            var metaDataIds = new List<int>();             
            foreach (var id in ids) CollectMetaDataIdsRecursively(id, ref metaDataIds);

            var deleteIds = ids.ToList<int>();
            if (metaDataIds.Any()) deleteIds.AddRange(metaDataIds);

            // check if we can delete entities with metadata, or throw exception
            var oks = BatchCheckCanDelete(deleteIds.ToArray(), force, skipIfCant, parentId, parentField);

            // than delete entities with metadata
            var ok = Parent.DataController.Entities.DeleteEntity(deleteIds.ToArray(), true, true);

            SystemManager.PurgeApp(Parent.AppId);

            return callLog.ReturnAndLog(ok);
        }

        private void CollectMetaDataIdsRecursively(int id, ref List<int> metaDataIds)
        {
            var childrenMetaDataIds = Parent.Read.Entities.Get(id).Metadata.Select(metdata => metdata.EntityId);
            if (!childrenMetaDataIds.Any()) return;
            foreach (var childrenMetadataId in childrenMetaDataIds)
            {
                CollectMetaDataIdsRecursively(childrenMetadataId, ref metaDataIds);
            }
            metaDataIds.AddRange(childrenMetaDataIds);
        }

        private Dictionary<int, Tuple<bool, string>> BatchCheckCanDelete(int[] ids, bool force, bool skipIfCant, int? parentId = null, string parentField = null)
        {
            var canDeleteList = CanDeleteEntitiesBasedOnAppStateRelationshipsOrMetadata(ids, parentId, parentField);

            foreach (var canDelete in canDeleteList)
                if (!canDelete.Value.Item1 && !force && !skipIfCant)
                {
                    var msg = $"Can't delete Item {canDelete.Key}. It is used by others. {canDelete.Value.Item2}";
                    Log.A(msg);
                    throw new InvalidOperationException(msg);
                }

            return canDeleteList;
        }

        private void BatchCheckTypesMatch(int[] ids, string contentType)
        {
            foreach (var id in ids)
            {
                var found = Parent.Read.Entities.Get(id);
                if (contentType != null && found.Type.Name != contentType && found.Type.NameId != contentType)
                    throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "', will not delete.");
            }
        }

        internal Tuple<bool, string> CanDeleteEntityBasedOnAppStateRelationshipsOrMetadata(int entityId, int? parentId = null, string parentField = null) 
            => CanDeleteEntitiesBasedOnAppStateRelationshipsOrMetadata(new[] {entityId}, parentId, parentField).First().Value;

        private Dictionary<int, Tuple<bool, string>> CanDeleteEntitiesBasedOnAppStateRelationshipsOrMetadata(int[] ids, int? parentId = null, string parentField = null)
        {
            var canDeleteList = new Dictionary<int, Tuple<bool, string>>();

            var relationships = Parent.Read.AppState.Relationships;

            foreach (var entityId in ids)
            {
                var messages = new List<string>();

                var parents = relationships.List.Where(r => r.Child.EntityId == entityId).ToList();

                // when have it, ignore first relation with part
                if (parentId.HasValue && !string.IsNullOrEmpty(parentField))
                {
                    var parentToIgnore = parents.FirstOrDefault(r => r.Parent.EntityId == parentId && r.Parent.Attributes.ContainsKey(parentField));
                    if (parentToIgnore != null) parents.Remove(parentToIgnore);
                }

                var parentsInfoForMessages = parents.Select(r => TryToGetMoreInfosAboutDependency(r.Parent)).ToList();

                if (parentsInfoForMessages.Any())
                    messages.Add(
                        $"Found {parentsInfoForMessages.Count} relationships where this is a child - the parents are: {string.Join(", ", parentsInfoForMessages)}.");

                /* Next part related to detection of children and metadata is commented because
                when an item has children or metadata, it can be deleted without force. 
                Force should only be required, if other items point to this. */

                //var children = relationships.List.Where(r => r.Parent.EntityId == entityId)
                //    .Select(r => TryToGetMoreInfosAboutDependency(r.Child)).ToList();

                //if (children.Any())
                //    messages.Add(
                //        $"Found {children.Count} entities which are assigned children: {string.Join(", ", children)}.");

                //var entity = Parent.Read.Entities.Get(entityId);

                //// check if entity has metadata
                //if (entity.Metadata.Any())
                //    messages.Add($"Found {entity.Metadata.Count()} metadata which are assigned.");

                //// check if entity is metadata
                //if (entity.MetadataFor?.IsMetadata ?? false)
                //    messages.Add($"Entity is metadata of other entity.");

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
            Log.A($"delete guid:{guid}");
            // todo: check if GetMostCurrentDbEntity... can't be in the app-layer
            // force: true - force-delete the data-source part
            return Delete(Parent.DataController.Entities.GetMostCurrentDbEntity(guid).EntityId, force: true);
        }

        public bool Delete(List<int> ids)
        {
            var callLog = Log.Fn<bool>($"ids:{ids.Count}", timer: true);
            var result = Delete(ids.ToArray(), null, false, true);
            //var result = ids.Aggregate(true, (current, entityId) => current && Delete(entityId, null, false, true));
            return callLog.ReturnAndLog(result);
        }

    }
    
}
