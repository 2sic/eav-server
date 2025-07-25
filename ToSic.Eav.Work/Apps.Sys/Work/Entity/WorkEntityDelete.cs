﻿using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.State.AppStateBuilder;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityDelete(Generator<IAppStateBuilder> stateBuilder)
    : WorkUnitBase<IAppWorkCtxWithDb>("Wrk.EntDel", connect: [stateBuilder])
{
    public bool Delete(Guid guid, bool force = false)
    {
        var l = Log.Fn<bool>($"delete guid:{guid}");
        // todo: check if GetMostCurrentDbEntity... can't be in the app-layer
        var idToDelete = AppWorkCtx.DbStorage.Entities.GetStandaloneDbEntityStub(guid).EntityId;
        return l.Return(Delete(idToDelete, force: force));
    }


    public bool Delete(int id, string? contentType = null, bool force = false, bool skipIfCant = false,
        int? parentId = null, string? parentField = null)
        => Delete([id], contentType, force, skipIfCant, parentId, parentField);

    public bool Delete(ICollection<int> ids)
        => Log.Quick(parameters: $"ids:{ids.Count}", timer: true, func:
            () => Delete([.. ids], null, false, true));


    /// <summary>
    /// delete an entity
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="contentType">optional content-type name to check before deleting</param>
    /// <param name="force">force delete even if there are relationships, resulting in removal of the relationships</param>
    /// <param name="skipIfCant">skip deleting if relationships exist and force is false</param>
    /// <param name="parentId"></param>
    /// <param name="parentField"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool Delete(int[] ids, string? contentType = null, bool force = false, bool skipIfCant = false, int? parentId = null, string? parentField = null) 
    {
        var l = Log.Fn<bool>($"delete id:{ids.Length}, type:{contentType}, force:{force}", timer: true);
        // do optional type-check and if necessary, throw error
        BatchCheckTypesMatch(ids, contentType);

        // get related metadata ids
        var metaDataIds = new List<int>();
        foreach (var id in ids) CollectMetaDataIdsRecursively(id, ref metaDataIds);

        var deleteIds = ids.ToList();
        if (metaDataIds.Count != 0) deleteIds.AddRange(metaDataIds);

        // check if we can delete entities with metadata, or throw exception
        var oks = BatchCheckCanDelete([.. deleteIds], force, skipIfCant, parentId, parentField);

        // then delete entities with metadata without app cache purge
        var repositoryIds = deleteIds.ToArray();
        var ok = false;
        var dc = AppWorkCtx.DbStorage;
        dc.DoButSkipAppCachePurge(() => ok = dc.Entities.DeleteEntity(repositoryIds, true, true));

        // remove entity from cache
        // introduced in v15.05 to reduce work on entity delete
        // in past we PurgeApp in whole on each entity delete
        // this should be much faster, but side effects are possible.
        var builder = stateBuilder.New().Init(AppWorkCtx.AppReader.GetCache());
        builder.RemoveEntities(repositoryIds, true);

        return l.Return(ok);
    }


    private void CollectMetaDataIdsRecursively(int id, ref List<int> metaDataIds)
    {
        var childrenMetaDataIds = AppWorkCtx.AppReader.List.FindRepoId(id)!
            .Metadata
            .Select(md => md.EntityId)
            .ToList();
        if (!childrenMetaDataIds.Any())
            return;
        foreach (var childrenMetadataId in childrenMetaDataIds)
            CollectMetaDataIdsRecursively(childrenMetadataId, ref metaDataIds);
        metaDataIds.AddRange(childrenMetaDataIds);
    }

    private Dictionary<int, (bool, string)> BatchCheckCanDelete(int[] ids, bool force, bool skipIfCant, int? parentId = null, string? parentField = null)
    {
        var canDeleteList = CanDeleteEntitiesBasedOnAppStateRelationshipsOrMetadata(ids, parentId, parentField);

        foreach (var canDelete in canDeleteList)
            if (!canDelete.Value.HasMessages && !force && !skipIfCant)
            {
                var msg = $"Can't delete Item {canDelete.Key}. It is used by others. {canDelete.Value.Messages}";
                Log.A(msg);
                throw new InvalidOperationException(msg);
            }

        return canDeleteList;
    }

    private void BatchCheckTypesMatch(int[] ids, string? contentType)
    {
        foreach (var id in ids)
        {
            var found = AppWorkCtx.AppReader.List.FindRepoId(id)!;
            if (contentType != null && found.Type.Name != contentType && found.Type.NameId != contentType)
                throw new KeyNotFoundException("Can't find " + id + "of type '" + contentType + "', will not delete.");
        }
    }

    internal (bool HasMessages, string Messages) CanDeleteEntityBasedOnAppStateRelationshipsOrMetadata(int entityId, int? parentId = null, string? parentField = null)
        => CanDeleteEntitiesBasedOnAppStateRelationshipsOrMetadata([entityId], parentId, parentField).First().Value;


    private Dictionary<int, (bool HasMessages, string Messages)> CanDeleteEntitiesBasedOnAppStateRelationshipsOrMetadata(int[] ids, int? parentId = null, string? parentField = null)
    {
        var canDeleteList = new Dictionary<int, (bool HasMessages, string Messages)>();

        var relationships = AppWorkCtx.AppReader.GetRelationships();

        foreach (var entityId in ids)
        {
            var messages = new List<string>();

            var parents = relationships.List
                .Where(r => r.Child.EntityId == entityId)
                .ToList();

            // when have it, ignore first relation with part
            if (parentId.HasValue && !string.IsNullOrEmpty(parentField))
            {
                var parentToIgnore = parents
                    .FirstOrDefault(r => r.Parent.EntityId == parentId && r.Parent.Attributes.ContainsKey(parentField!));
                if (parentToIgnore != null)
                    parents.Remove(parentToIgnore);
            }

            var parentsInfoForMessages = parents
                .Select(r => TryToGetMoreInfosAboutDependency(r.Parent))
                .ToList();

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

            canDeleteList.Add(entityId, (!messages.Any(), string.Join(" ", messages)));
        }

        if (canDeleteList.Count != ids.Length)
            throw new("Delete check failed, results doesn't match request");

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

}