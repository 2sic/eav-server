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

    private sealed record HistoryDeletedEntityRow(int EntityId, Guid? EntityGuid, int DeletedTransactionId, DateTime Timestamp, string? Json, byte[]? CJson);

    public IReadOnlyList<RecycleBinItem> Get()
    {
        var l = Log.Fn<IReadOnlyList<RecycleBinItem>>();


        var appId = AppWorkCtx.AppId;
        var parentRef = DbVersioning.ParentRefForApp(appId);
        var db = AppWorkCtx.DbStorage.SqlDb;

        var softDeletedItems = LoadSoftDeletedRecycleBinItems(db, appId, parentRef);
        var historyOnlyItems = LoadHistoryOnlyRecycleBinItems(db, appId, parentRef);

        var items = softDeletedItems
            .Concat(historyOnlyItems)
            .OrderByDescending(i => i.DeletedUtc)
            .ToList();

        return l.Return(items, $"found:{items.Count}");
    }

    private static List<RecycleBinItem> LoadSoftDeletedRecycleBinItems(EavDbContext db, int appId, string? parentRef)
        => db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId && e.TransDeletedId != null)
            .GroupJoin(
                db.TsDynDataTransactions.AsNoTracking(),
                e => e.TransDeletedId,
                t => t.TransactionId,
                (e, txJoin) => new { e, txJoin })
            .SelectMany(
                t => t.txJoin.DefaultIfEmpty(),
                (t, tx) => new { t.e, tx })
            .GroupJoin(
                db.TsDynDataContentTypes.AsNoTracking().IgnoreQueryFilters(),
                et => et.e.ContentTypeId,
                ct => ct.ContentTypeId,
                (et, ctJoin) => new { et.e, et.tx, ctJoin })
            .SelectMany(
                t => t.ctJoin.DefaultIfEmpty(),
                (t, ct) => new RecycleBinItem(
                    t.e.EntityId,
                    t.e.EntityGuid,
                    t.e.AppId,
                    ct != null ? ct.StaticName : (t.e.ContentType ?? ""),
                    ct != null ? ct.Name : (t.e.ContentType ?? ""),
                    t.e.TransDeletedId!.Value,
                    (t.tx == null ? (DateTime?)null : t.tx.Timestamp) ?? DateTime.MinValue,
                    t.tx != null ? t.tx.User : null,
                    parentRef))
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

        return db.TsDynDataHistories
            .AsNoTracking()
            .Where(h => h.SourceTable == EntitiesTableName
                && h.Operation == EavConstants.HistoryEntityJson
                && h.ParentRef == parentRef
                && h.SourceId != null
                && h.SourceGuid != null
                && h.TransactionId != null
                && !entityIdsInApp.Contains(h.SourceId.Value)
                && !entityGuidsInApp.Contains(h.SourceGuid.Value))
            .GroupBy(h => h.SourceGuid)
            .Select(g => g.OrderByDescending(h => h.Timestamp).First())
            .Select(h => new HistoryDeletedEntityRow(
                h.SourceId!.Value,
                h.SourceGuid,
                h.TransactionId!.Value,
                h.Timestamp,
                h.Json,
                h.CJson))
            .ToList();
    }

    private List<RecycleBinItem> LoadHistoryOnlyRecycleBinItems(EavDbContext db, int appId, string? parentRef)
    {
        var historyRows = LoadHistoryDeletedEntitiesLatest(db, appId, parentRef);
        if (historyRows.Count == 0)
            return [];

        var txIds = historyRows
            .Select(h => h.DeletedTransactionId)
            .Distinct()
            .ToList();

        var transactions = db.TsDynDataTransactions
            .AsNoTracking()
            .Where(t => txIds.Contains(t.TransactionId))
            .Select(t => new { t.TransactionId, t.Timestamp, t.User })
            .ToDictionary(t => t.TransactionId, t => (t.Timestamp, t.User));

        return historyRows
            .Where(h => h.EntityGuid != Guid.Empty)
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
                    DeletedUtc: tx.Timestamp != default ? tx.Timestamp : h.Timestamp,
                    DeletedBy: tx.User,
                    ParentRef: parentRef);
            })
            .ToList();
    }

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
