using Microsoft.EntityFrameworkCore;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils;
using ToSic.Sys.Utils.Compression;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityRecycleBin(LazySvc<Compressor> compressor, Generator<JsonSerializer> jsonSerializer)
    : WorkUnitBase<IAppWorkCtxWithDb>("Wrk.EntRcy", connect: [compressor, jsonSerializer])
{
    /// <summary>
    /// Old SQL name, which is still used as a key in the history table, so we need to keep it here as a constant.
    /// </summary>
    private const string EntitiesTableName = "ToSIC_EAV_Entities";

    public sealed record RecycleBinItem(
        int Id,
        Guid Guid,
        int AppId,
        string TypeNameId,
        string TypeName,
        int TransactionId,
        DateTime Deleted,
        string? DeletedBy,
        string? ParentRef
    )
    {
        public string? Json { get; init; }
        public DateTime? FilterDateFrom { get; init; }
        public DateTime? FilterDateTo { get; init; }
        public string? FilterContentType { get; init; }
    };

    private sealed record HistoryDeletedEntityRow(int EntityId, Guid? EntityGuid, int DeletedTransactionId, DateTime Timestamp, string? Json, byte[]? CJson);

    public IReadOnlyList<RecycleBinItem> Get(DateTime? dateFrom = null, DateTime? dateTo = null, string? contentType = null)
    {
        var l = Log.Fn<IReadOnlyList<RecycleBinItem>>();

        var appId = AppWorkCtx.AppId;
        var appRefId = DbVersioning.ParentRefForApp(appId)!;
        var db = AppWorkCtx.DbStorage.SqlDb;

        var softDeletedItems = LoadSoftDeletedRecycleBinItems(db, appId, appRefId, dateFrom, dateTo, contentType);
        var historyOnlyItems = LoadHistoryOnlyRecycleBinItems(db, appId, appRefId, dateFrom, dateTo, contentType);

        var items = softDeletedItems
            .Concat(historyOnlyItems)
            .OrderByDescending(i => i.Deleted)
            .ToList();

        return l.Return(items, $"found:{items.Count}");
    }

    private static List<RecycleBinItem> LoadSoftDeletedRecycleBinItems(EavDbContext db, int appId, string appRefId, DateTime? dateFrom, DateTime? dateTo, string? contentType)
    {
        var baseQuery = TsDynDataEntitiesBaseQuery(db, appId);

        return baseQuery
            .Where(e => e.TransDeletedId != null)
            .GroupJoin(
                db.TsDynDataTransactions.AsNoTracking(),
                ent => ent.TransDeletedId,
                tx => tx.TransactionId,
                (ent, txJoin) => new { ent, txJoin })
            .SelectMany(
                t => t.txJoin.DefaultIfEmpty(),
                (t, tx) => new { t.ent, tx })
            .Where(pair => !dateFrom.HasValue || (pair.tx != null && pair.tx.Timestamp >= dateFrom.Value))
            .Where(pair => !dateTo.HasValue || (pair.tx != null && pair.tx.Timestamp <= dateTo.Value))
            .GroupJoin(
                db.TsDynDataContentTypes.AsNoTracking().IgnoreQueryFilters(),
                pair => pair.ent.ContentTypeId,
                ct => ct.ContentTypeId,
                (pair, ctJoin) => new { pair.ent, pair.tx, ctJoin })
            .SelectMany(
                txSet => txSet.ctJoin.DefaultIfEmpty(),
                (txSet, ct) => new
                {
                    txSet.ent,
                    txSet.tx,
                    ct,
                    ContentTypeStaticName = ct != null ? ct.StaticName : (txSet.ent.ContentType ?? ""),
                    ContentTypeName = ct != null ? ct.Name : (txSet.ent.ContentType ?? "")
                })
            .Where(txBundle => string.IsNullOrWhiteSpace(contentType)
                        || txBundle.ContentTypeStaticName == contentType
                        || txBundle.ContentTypeName == contentType)
            .Select(txBundle => new RecycleBinItem(
                Id: txBundle.ent.EntityId,
                Guid: txBundle.ent.EntityGuid,
                AppId: txBundle.ent.AppId,
                TypeNameId: txBundle.ContentTypeStaticName,
                TypeName: txBundle.ContentTypeName,
                TransactionId: txBundle.ent.TransDeletedId!.Value,
                Deleted: (txBundle.tx == null ? (DateTime?)null : txBundle.tx.Timestamp) ?? DateTime.MinValue,
                txBundle.tx != null ? txBundle.tx.User : null,
                appRefId)
            {
                Json = txBundle.ent.Json,
                FilterDateFrom = dateFrom,
                FilterDateTo = dateTo,
                FilterContentType = contentType
            })
            .ToList();
    }

    private static List<HistoryDeletedEntityRow> LoadHistoryDeletedEntitiesLatest(EavDbContext db, int appId, string appRefId, DateTime? dateFrom, DateTime? dateTo)
    {
        var baseQuery = TsDynDataEntitiesBaseQuery(db, appId);

        var entityIdsInApp = baseQuery
            .Select(e => e.EntityId);

        var entityGuidsInApp = baseQuery
            .Select(e => e.EntityGuid);

        return db.TsDynDataHistories
            .AsNoTracking()
            .Where(h => h.SourceTable == EntitiesTableName
                && h.Operation == EavConstants.HistoryEntityJson
                && h.ParentRef == appRefId
                && h.SourceId != null
                && h.SourceGuid != null
                && h.TransactionId != null
                && !entityIdsInApp.Contains(h.SourceId.Value)
                && !entityGuidsInApp.Contains(h.SourceGuid.Value))
            .Where(h => !dateFrom.HasValue || h.Timestamp >= dateFrom.Value)
            .Where(h => !dateTo.HasValue || h.Timestamp <= dateTo.Value)
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

    private static IQueryable<TsDynDataEntity> TsDynDataEntitiesBaseQuery(EavDbContext db, int appId)
    {
        var baseQuery = db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId);
        return baseQuery;
    }

    private List<RecycleBinItem> LoadHistoryOnlyRecycleBinItems(EavDbContext db, int appId, string parentRef, DateTime? dateFrom, DateTime? dateTo, string? contentType)
    {
        var historyRows = LoadHistoryDeletedEntitiesLatest(db, appId, parentRef, dateFrom, dateTo);
        if (historyRows.Count == 0)
            return [];

        var txIds = historyRows
            .Select(h => h.DeletedTransactionId)
            .Distinct()
            .ToList();

        var transactions = db.TsDynDataTransactions
            .AsNoTracking()
            .Where(tx => txIds.Contains(tx.TransactionId))
            .Select(tx => new { tx.TransactionId, tx.Timestamp, tx.User })
            .ToDictionary(
                txSet => txSet.TransactionId,
                txSet => (txSet.Timestamp, txSet.User)
            );

        return historyRows
            .Where(h => h.EntityGuid != Guid.Empty)
            .Select(h =>
            {
                transactions.TryGetValue(h.DeletedTransactionId, out var tx);

                var json = ResolveHistoryJson(h);
                var entity = TryDeserializeEntity(json);

                return new RecycleBinItem(
                    Id: h.EntityId,
                    Guid: h.EntityGuid ?? entity?.EntityGuid ?? Guid.Empty,
                    AppId: appId,
                    TypeNameId: entity?.Type.NameId ?? "",
                    TypeName: entity?.Type.Name ?? "",
                    TransactionId: h.DeletedTransactionId,
                    Deleted: tx.Timestamp != default ? tx.Timestamp : h.Timestamp,
                    DeletedBy: tx.User,
                    ParentRef: parentRef)
                {
                    Json = json,
                    FilterDateFrom = dateFrom,
                    FilterDateTo = dateTo,
                    FilterContentType = contentType
                };
            })
            .Where(i => string.IsNullOrWhiteSpace(contentType) 
                || string.Equals(i.TypeNameId, contentType, StringComparison.OrdinalIgnoreCase) 
                || string.Equals(i.TypeName, contentType, StringComparison.OrdinalIgnoreCase))
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
