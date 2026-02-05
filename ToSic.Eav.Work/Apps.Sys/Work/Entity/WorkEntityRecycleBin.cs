using Microsoft.EntityFrameworkCore;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils;
using ToSic.Sys.Utils.Compression;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityRecycleBin(
    LazySvc<Compressor> compressor,
    Generator<JsonSerializer> jsonSerializer)
    : WorkUnitBase<IAppWorkCtxWithDb>("Wrk.EntRcy", connect: [compressor, jsonSerializer])
{
    private const string EntitiesTableName = "ToSIC_EAV_Entities";

    public sealed record RecycleBinItem(
        int EntityId,
        Guid EntityGuid,
        int AppId,
        string ContentTypeStaticName,
        string ContentTypeName,
        int DeletedTransactionId,
        DateTime DeletedUtc,
        string? DeletedBy,
        string? ParentRef
    );

    private sealed record SoftDeletedEntityRow(int EntityId, Guid EntityGuid, int AppId, int ContentTypeId, string? ContentType, int DeletedTransactionId);

    private sealed record HistoryDeletedEntityRow(int EntityId, Guid? EntityGuid, int DeletedTransactionId, DateTime Timestamp, string? Json, byte[]? CJson);

    private sealed record TransactionInfo(DateTime Timestamp, string? User);

    private sealed record ContentTypeInfo(string StaticName, string Name);

    public IReadOnlyList<RecycleBinItem> Get()
    {
        var l = Log.Fn<IReadOnlyList<RecycleBinItem>>();


        var appId = AppWorkCtx.AppId;
        var parentRef = DbVersioning.ParentRefForApp(appId);
        var db = AppWorkCtx.DbStorage.SqlDb;

        var deletedEntities = LoadSoftDeletedEntities(db, appId);
        var historyDeletedEntitiesLatest = LoadHistoryDeletedEntitiesLatest(db, appId, parentRef);

        var transactions = LoadTransactionsById(db, deletedEntities, historyDeletedEntitiesLatest);
        var contentTypes = LoadContentTypesById(db, deletedEntities);

        var softDeletedItems = BuildSoftDeletedItems(deletedEntities, transactions, contentTypes, parentRef);
        var historyOnlyItems = BuildHistoryOnlyItems(appId, historyDeletedEntitiesLatest, transactions, parentRef);

        var items = softDeletedItems
            .Concat(historyOnlyItems)
            .OrderByDescending(i => i.DeletedUtc)
            .ToList();

        return l.Return(items, $"found:{items.Count}");
    }

    private static List<SoftDeletedEntityRow> LoadSoftDeletedEntities(EavDbContext db, int appId)
        => db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId && e.TransDeletedId != null)
            .Select(e => new SoftDeletedEntityRow(
                e.EntityId,
                e.EntityGuid,
                e.AppId,
                e.ContentTypeId,
                e.ContentType,
                e.TransDeletedId!.Value))
            .ToList();

    private static List<HistoryDeletedEntityRow> LoadHistoryDeletedEntitiesLatest(EavDbContext db, int appId, string? parentRef)
    {
        var entityIdsInApp = db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId)
            .Select(e => e.EntityId);

        var entityGuidsInApp = db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId)
            .Select(e => e.EntityGuid);

        var historyMissingEntityRows = db.TsDynDataHistories
            .AsNoTracking()
            .Where(h => h.SourceTable == EntitiesTableName
                && h.Operation == EavConstants.HistoryEntityJson
                && h.ParentRef == parentRef
                && h.SourceId != null
                && h.SourceGuid != null
                && h.TransactionId != null
                && !entityIdsInApp.Contains(h.SourceId.Value)
                && !entityGuidsInApp.Contains(h.SourceGuid.Value))
            .OrderByDescending(h => h.Timestamp)
            .Select(h => new HistoryDeletedEntityRow(
                h.SourceId!.Value,
                h.SourceGuid,
                h.TransactionId!.Value,
                h.Timestamp,
                h.Json,
                h.CJson))
            .ToList();

        return historyMissingEntityRows
            .GroupBy(h => h.EntityId)
            .Select(g => g.First())
            .ToList();
    }

    private static Dictionary<int, TransactionInfo> LoadTransactionsById(EavDbContext db, List<SoftDeletedEntityRow> deletedEntities, List<HistoryDeletedEntityRow> historyDeletedEntitiesLatest)
    {
        var deletedTransactionIds = deletedEntities
            .Select(e => e.DeletedTransactionId)
            .Concat(historyDeletedEntitiesLatest.Select(h => h.DeletedTransactionId))
            .Distinct()
            .ToList();

        if (deletedTransactionIds.Count == 0)
            return [];

        return db.TsDynDataTransactions
            .AsNoTracking()
            .Where(t => deletedTransactionIds.Contains(t.TransactionId))
            .Select(t => new { t.TransactionId, t.Timestamp, t.User })
            .ToList()
            .ToDictionary(t => t.TransactionId, t => new TransactionInfo(t.Timestamp, t.User));
    }

    private static Dictionary<int, ContentTypeInfo> LoadContentTypesById(EavDbContext db, List<SoftDeletedEntityRow> deletedEntities)
    {
        var contentTypeIds = deletedEntities
            .Select(e => e.ContentTypeId)
            .Distinct()
            .ToList();

        if (contentTypeIds.Count == 0)
            return [];

        return db.TsDynDataContentTypes
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(ct => contentTypeIds.Contains(ct.ContentTypeId))
            .Select(ct => new { ct.ContentTypeId, ct.StaticName, ct.Name })
            .ToList()
            .ToDictionary(ct => ct.ContentTypeId, ct => new ContentTypeInfo(ct.StaticName, ct.Name));
    }

    private static List<RecycleBinItem> BuildSoftDeletedItems(
        List<SoftDeletedEntityRow> deletedEntities,
        Dictionary<int, TransactionInfo> transactions,
        Dictionary<int, ContentTypeInfo> contentTypes,
        string? parentRef)
        => deletedEntities
            .Select(e =>
            {
                transactions.TryGetValue(e.DeletedTransactionId, out var tx);
                contentTypes.TryGetValue(e.ContentTypeId, out var ct);

                return new RecycleBinItem(
                    EntityId: e.EntityId,
                    EntityGuid: e.EntityGuid,
                    AppId: e.AppId,
                    ContentTypeStaticName: ct?.StaticName ?? e.ContentType ?? "",
                    ContentTypeName: ct?.Name ?? e.ContentType ?? "",
                    DeletedTransactionId: e.DeletedTransactionId,
                    DeletedUtc: tx?.Timestamp ?? DateTime.MinValue,
                    DeletedBy: tx?.User,
                    ParentRef: parentRef);
            })
            .ToList();

    private List<RecycleBinItem> BuildHistoryOnlyItems(
        int appId,
        List<HistoryDeletedEntityRow> historyDeletedEntitiesLatest,
        Dictionary<int, TransactionInfo> transactions,
        string? parentRef)
        => historyDeletedEntitiesLatest
            .Where(i => i.EntityGuid != Guid.Empty)
            .Select(h =>
            {
                transactions.TryGetValue(h.DeletedTransactionId, out var tx);

                var json = ResolveHistoryJson(h);
                var entity = TryDeserializeEntity(json);

                return new RecycleBinItem(
                    EntityId: h.EntityId,
                    EntityGuid: h.EntityGuid ?? entity?.EntityGuid ?? Guid.Empty,
                    AppId: appId,
                    ContentTypeStaticName: entity?.Type.NameId ?? "",
                    ContentTypeName: entity?.Type.Name ?? "",
                    DeletedTransactionId: h.DeletedTransactionId,
                    DeletedUtc: tx?.Timestamp ?? h.Timestamp,
                    DeletedBy: tx?.User,
                    ParentRef: parentRef);
            })
            .ToList();

    private string? ResolveHistoryJson(HistoryDeletedEntityRow row)
        => string.IsNullOrEmpty(row.Json)
            ? (row.CJson == null ? null : Decompress(row.CJson))
            : row.Json;

    private IEntity? TryDeserializeEntity(string? json)
    {
        if (!json.HasValue())
            return null;

        try
        {
            var serializer = jsonSerializer.New();
            serializer.Initialize(AppWorkCtx.AppReader);
            return serializer.Deserialize(json);
        }
        catch
        {
            // ignore parse issues - we'll still return a minimal item
            return null;
        }
    }

    private string? Decompress(byte[] bytes)
        => compressor.Value.DecompressOrNullIfDisabled(bytes)
           ?? new Compressor().DecompressOrNullIfDisabled(bytes);
}
