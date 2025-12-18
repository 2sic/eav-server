using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
using ToSic.Eav.Serialization.Sys;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils.Compression;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityRecycle(
    AppCachePurger appCachePurger,
    LazySvc<ImportService> import,
    LazySvc<JsonSerializer> jsonSerializer,
    LazySvc<Compressor> compressor)
    : WorkUnitBase<IAppWorkCtxWithDb>("Wrk.EntRcl", connect: [appCachePurger, import, jsonSerializer, compressor])
{
    private const string EntitiesTableName = "ToSIC_EAV_Entities";

    public void Recycle(int appId, int transactionId)
    {
        var l = Log.Fn($"appId:{appId}, tx:{transactionId}");

        ValidateInputs(appId, transactionId);

        var db = AppWorkCtx.DbStorage.SqlDb;
        EnsureTransactionExists(db, transactionId);

        var softDeletedEntities = LoadSoftDeletedEntities(db, appId, transactionId);
        UndeleteSoftDeletedEntities(softDeletedEntities);

        var historySnapshots = LoadHistoryOnlyEntitySnapshots(db, appId, transactionId);
        EnsureSomethingToRecycle(appId, transactionId, softDeletedEntities.Count, historySnapshots.Count);

        var historyJsonEntities = RestoreMissingEntitiesFromHistory(historySnapshots);

        var restoredEntitiesByGuid = LoadRestoredEntitiesByGuid(db, appId, softDeletedEntities, historySnapshots);
        RestoreRelationshipsWhereEntityWasChild(db, restoredEntitiesByGuid);
        RestoreRelationshipsWhereEntityWasParent(db, restoredEntitiesByGuid);

        if (historyJsonEntities.Count > 0)
            RestoreParentRelationshipsFromHistory(appId, historyJsonEntities, restoredEntitiesByGuid);

        db.SaveChanges();
        appCachePurger.Purge(AppWorkCtx);

        l.Done();
    }

    private static void ValidateInputs(int appId, int transactionId)
    {
        if (appId <= 0)
            throw new ArgumentOutOfRangeException(nameof(appId));

        if (transactionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(transactionId));
    }

    private static void EnsureTransactionExists(EavDbContext db, int transactionId)
    {
        var transactionExists = db.TsDynDataTransactions
            .AsNoTracking()
            .Any(t => t.TransactionId == transactionId);

        if (!transactionExists)
            throw new InvalidOperationException($"Transaction {transactionId} not found.");
    }

    private static List<TsDynDataEntity> LoadSoftDeletedEntities(EavDbContext db, int appId, int transactionId)
        => db.TsDynDataEntities
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId && e.TransDeletedId == transactionId)
            .ToList();

    private static void UndeleteSoftDeletedEntities(List<TsDynDataEntity> softDeletedEntities)
    {
        foreach (var entity in softDeletedEntities)
            entity.TransDeletedId = null;
    }

    private List<HistoryEntitySnapshot> LoadHistoryOnlyEntitySnapshots(EavDbContext db, int appId, int transactionId)
    {
        // Hard-deleted entities won't exist in TsDynDataEntities anymore.
        // They are stored in TsDynDataHistory as Json/CJson.
        var parentRef = DbVersioning.ParentRefForApp(appId);

        var existingEntityIdsInApp = db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId)
            .Select(e => e.EntityId);

        var candidates = db.TsDynDataHistories
            .AsNoTracking()
            .Where(h => h.SourceTable == EntitiesTableName
                && h.Operation == EavConstants.HistoryEntityJson
                && h.ParentRef == parentRef
                && h.TransactionId == transactionId
                && h.SourceId != null
                && !existingEntityIdsInApp.Contains(h.SourceId.Value))
            .OrderByDescending(h => h.Timestamp)
            .Select(h => new HistoryEntitySnapshot(h.SourceId!.Value, h.SourceGuid, h.Json, h.CJson))
            .ToList();

        // One entity can have multiple history entries; use the newest per SourceId.
        return candidates
            .GroupBy(h => h.EntityId)
            .Select(g => g.First())
            .ToList();
    }

    private static void EnsureSomethingToRecycle(int appId, int transactionId, int softDeletedCount, int historyOnlyCount)
    {
        if (softDeletedCount == 0 && historyOnlyCount == 0)
            throw new InvalidOperationException($"No deleted entities found in app {appId} for transaction {transactionId}.");
    }

    private List<JsonEntity> RestoreMissingEntitiesFromHistory(List<HistoryEntitySnapshot> historySnapshots)
    {
        if (historySnapshots.Count == 0)
            return [];

        var serializer = jsonSerializer.Value.SetApp(AppWorkCtx.AppReader);
        var importer = import.Value.Init(AppWorkCtx.ZoneId, AppWorkCtx.AppId, skipExistingAttributes: false, preserveUntouchedAttributes: false);

        var jsonEntities = new List<JsonEntity>();
        var entitiesFromHistory = new List<Entity>();

        foreach (var snapshot in historySnapshots)
        {
            var json = snapshot.Json ?? string.Empty;
            if (json.Length == 0 && snapshot.CJson != null)
                json = DecompressRegardlessOfFeatureFlag(snapshot.CJson) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(json))
                continue;

            try
            {
                var jsonEntity = serializer.UnpackEntityAndTestGenericJsonV1(json);
                jsonEntities.Add(jsonEntity);

                var entity = serializer.Deserialize(jsonEntity, allowDynamic: false, skipUnknownType: false) as Entity;
                if (entity != null)
                    entitiesFromHistory.Add(entity with { EntityId = 0, RepositoryId = 0 });
            }
            catch
            {
                // ignore invalid history json
            }
        }

        if (entitiesFromHistory.Count > 0)
            importer.ImportIntoDb([], entitiesFromHistory);

        return jsonEntities;
    }

    private static Dictionary<Guid, int> LoadRestoredEntitiesByGuid(
        EavDbContext db,
        int appId,
        List<TsDynDataEntity> softDeletedEntities,
        List<HistoryEntitySnapshot> historySnapshots)
    {
        var restoredGuids = softDeletedEntities
            .Select(e => e.EntityGuid)
            .Concat(historySnapshots.Select(h => h.SourceGuid).Where(g => g != null).Cast<Guid>())
            .Distinct()
            .ToList();

        if (restoredGuids.Count == 0)
            return new();

        return db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId && restoredGuids.Contains(e.EntityGuid))
            .Select(e => new { e.EntityGuid, e.EntityId })
            .ToList()
            .GroupBy(x => x.EntityGuid)
            .ToDictionary(g => g.Key, g => g.First().EntityId);
    }

    private static void RestoreRelationshipsWhereEntityWasChild(EavDbContext db, Dictionary<Guid, int> restoredEntitiesByGuid)
    {
        var restoredGuids = restoredEntitiesByGuid.Keys.ToList();
        if (restoredGuids.Count == 0)
            return;

        var relationshipsWhereEntityWasChild = db.TsDynDataRelationships
            .Where(r => r.ChildEntityId == null
                && r.ChildExternalId != null
                && restoredGuids.Contains(r.ChildExternalId.Value))
            .ToList();

        foreach (var relationship in relationshipsWhereEntityWasChild)
        {
            if (!relationship.ChildExternalId.HasValue)
                continue;

            if (!restoredEntitiesByGuid.TryGetValue(relationship.ChildExternalId.Value, out var restoredEntityId))
                continue;

            relationship.ChildEntityId = restoredEntityId;
            relationship.ChildExternalId = null;
        }
    }

    private static void RestoreRelationshipsWhereEntityWasParent(EavDbContext db, Dictionary<Guid, int> restoredEntitiesByGuid)
    {
        var restoredEntityIds = restoredEntitiesByGuid.Values.Distinct().ToList();
        if (restoredEntityIds.Count == 0)
            return;

        // In this case, delete stored the child GUID in ChildExternalId and cleared ChildEntityId.
        var relationshipsWhereEntityWasParent = db.TsDynDataRelationships
            .Where(r => restoredEntityIds.Contains(r.ParentEntityId)
                && r.ChildEntityId == null
                && r.ChildExternalId != null)
            .ToList();

        var childGuidsToResolve = relationshipsWhereEntityWasParent
            .Where(r => r.ChildExternalId.HasValue)
            .Select(r => r.ChildExternalId!.Value)
            .Distinct()
            .ToList();

        var childEntityIdsByGuid = childGuidsToResolve.Count == 0
            ? new Dictionary<Guid, int>()
            : db.TsDynDataEntities
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(e => childGuidsToResolve.Contains(e.EntityGuid))
                .Select(e => new { e.EntityGuid, e.EntityId })
                .ToList()
                .GroupBy(x => x.EntityGuid)
                .ToDictionary(g => g.Key, g => g.First().EntityId);

        foreach (var relationship in relationshipsWhereEntityWasParent)
        {
            if (!relationship.ChildExternalId.HasValue)
                continue;

            if (!childEntityIdsByGuid.TryGetValue(relationship.ChildExternalId.Value, out var childEntityId))
                continue;

            relationship.ChildEntityId = childEntityId;
            relationship.ChildExternalId = null;
        }
    }

    private string? DecompressRegardlessOfFeatureFlag(byte[] bytes)
        => compressor.Value.Decompress(bytes)
           ?? new Compressor().Decompress(bytes);

    private sealed record HistoryEntitySnapshot(int EntityId, Guid? SourceGuid, string? Json, byte[]? CJson);

    private void RestoreParentRelationshipsFromHistory(int appId, List<JsonEntity> historyJsonEntities, Dictionary<Guid, int> restoredEntitiesByGuid)
    {
        var l = Log.Fn(timer: true);

        var entitiesWithParents = FilterEntitiesWithParents(historyJsonEntities);
        if (entitiesWithParents.Count == 0)
            return;

        var db = AppWorkCtx.DbStorage.SqlDb;

        var childIdsByGuid = BuildChildIdsByGuid(appId, db, entitiesWithParents, restoredEntitiesByGuid);
        if (childIdsByGuid.Count == 0)
            return;

        var parentGuids = CollectParentGuids(entitiesWithParents);
        if (parentGuids.Count == 0)
            return;

        var parentsByGuid = LoadParentsByGuid(appId, db, parentGuids);
        if (parentsByGuid.Count == 0)
            return;

        var attributesByType = LoadAttributeIdsByType(db, parentsByGuid);
        if (attributesByType.Count == 0)
            return;

        var specs = BuildRelationshipSpecs(entitiesWithParents, childIdsByGuid, parentsByGuid, attributesByType);
        if (specs.Count == 0)
            return;

        var existingKeys = LoadExistingRelationshipKeys(db, specs);
        AddMissingRelationships(db, specs, existingKeys);

        l.Done();
    }

    private static List<JsonEntity> FilterEntitiesWithParents(List<JsonEntity> historyJsonEntities)
        => historyJsonEntities
            .Where(e => e.Parents is { Count: > 0 })
            .ToList();

    private static Dictionary<Guid, int> BuildChildIdsByGuid(int appId, EavDbContext db, List<JsonEntity> entitiesWithParents, Dictionary<Guid, int> restoredEntitiesByGuid)
    {
        var childGuids = entitiesWithParents
            .Select(e => e.Guid)
            .Distinct()
            .ToList();

        var childIdsByGuid = childGuids
            .Where(restoredEntitiesByGuid.ContainsKey)
            .ToDictionary(g => g, g => restoredEntitiesByGuid[g]);

        // If the caller-provided map doesn't contain the entity yet (depends on import/save timing), resolve from DB.
        var missingChildGuids = childGuids
            .Where(g => !childIdsByGuid.ContainsKey(g))
            .ToList();

        if (missingChildGuids.Count == 0)
            return childIdsByGuid;

        var resolved = db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId && missingChildGuids.Contains(e.EntityGuid))
            .Select(e => new { e.EntityGuid, e.EntityId, e.IsPublished })
            .ToList()
            .GroupBy(e => e.EntityGuid)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(x => x.IsPublished)
                    .Select(x => x.EntityId)
                    .First());

        foreach (var kvp in resolved)
        {
            if (!childIdsByGuid.ContainsKey(kvp.Key))
                childIdsByGuid[kvp.Key] = kvp.Value;
        }

        return childIdsByGuid;
    }

    private static List<Guid> CollectParentGuids(List<JsonEntity> entitiesWithParents)
        => entitiesWithParents
            .SelectMany(e => e.Parents ?? [])
            .Select(p => p.Parent)
            .Distinct()
            .ToList();

    private sealed record ParentLookup(int EntityId, int ContentTypeId);

    private static Dictionary<Guid, ParentLookup> LoadParentsByGuid(int appId, EavDbContext db, List<Guid> parentGuids)
        => db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId && parentGuids.Contains(e.EntityGuid))
            .Select(e => new { e.EntityGuid, e.EntityId, e.ContentTypeId })
            .ToList()
            .GroupBy(p => p.EntityGuid)
            .ToDictionary(g => g.Key, g => new ParentLookup(g.First().EntityId, g.First().ContentTypeId));

    private static Dictionary<int, Dictionary<string, int>> LoadAttributeIdsByType(EavDbContext db, Dictionary<Guid, ParentLookup> parentsByGuid)
    {
        var parentContentTypeIds = parentsByGuid.Values
            .Select(p => p.ContentTypeId)
            .Distinct()
            .ToList();

        if (parentContentTypeIds.Count == 0)
            return [];

        return db.TsDynDataAttributes
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(a => parentContentTypeIds.Contains(a.ContentTypeId) && a.TransDeletedId == null)
            .Select(a => new { a.ContentTypeId, a.StaticName, a.AttributeId })
            .ToList()
            .GroupBy(a => a.ContentTypeId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .GroupBy(x => x.StaticName, StringComparer.InvariantCultureIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First().AttributeId, StringComparer.InvariantCultureIgnoreCase));
    }

    private static List<(int ParentId, int ChildId, int AttributeId, int SortOrder)> BuildRelationshipSpecs(
        List<JsonEntity> entitiesWithParents,
        Dictionary<Guid, int> childIdsByGuid,
        Dictionary<Guid, ParentLookup> parentsByGuid,
        Dictionary<int, Dictionary<string, int>> attributesByType)
    {
        var specs = new List<(int ParentId, int ChildId, int AttributeId, int SortOrder)>();

        foreach (var child in entitiesWithParents)
        {
            if (!childIdsByGuid.TryGetValue(child.Guid, out var childId))
                continue;

            foreach (var p in child.Parents ?? [])
            {
                if (!parentsByGuid.TryGetValue(p.Parent, out var parent))
                    continue;

                if (!attributesByType.TryGetValue(parent.ContentTypeId, out var attribsOfType))
                    continue;

                if (!attribsOfType.TryGetValue(p.Field, out var attributeId))
                    continue;

                specs.Add((parent.EntityId, childId, attributeId, p.SortOrder));
            }
        }

        return specs;
    }

    private static HashSet<(int ParentId, int ChildId, int AttributeId, int SortOrder)> LoadExistingRelationshipKeys(EavDbContext db, List<(int ParentId, int ChildId, int AttributeId, int SortOrder)> specs)
    {
        var parentIds = specs.Select(s => s.ParentId).Distinct().ToList();
        var childIds = specs.Select(s => s.ChildId).Distinct().ToList();
        var attributeIds = specs.Select(s => s.AttributeId).Distinct().ToList();

        return db.TsDynDataRelationships
            .AsNoTracking()
            .Where(r => r.ChildEntityId != null
                && parentIds.Contains(r.ParentEntityId)
                && childIds.Contains(r.ChildEntityId.Value)
                && attributeIds.Contains(r.AttributeId))
            .Select(r => new { r.ParentEntityId, ChildId = r.ChildEntityId!.Value, r.AttributeId, r.SortOrder })
            .ToList()
            .Select(r => (r.ParentEntityId, r.ChildId, r.AttributeId, r.SortOrder))
            .ToHashSet();
    }

    private static void AddMissingRelationships(EavDbContext db, List<(int ParentId, int ChildId, int AttributeId, int SortOrder)> specs, HashSet<(int ParentId, int ChildId, int AttributeId, int SortOrder)> existingKeys)
    {
        foreach (var s in specs)
        {
            if (existingKeys.Contains(s))
                continue;

            db.TsDynDataRelationships.Add(new TsDynDataRelationship
            {
                ParentEntityId = s.ParentId,
                ChildEntityId = s.ChildId,
                AttributeId = s.AttributeId,
                SortOrder = s.SortOrder,
                ChildExternalId = null,
            });
        }
    }
}
