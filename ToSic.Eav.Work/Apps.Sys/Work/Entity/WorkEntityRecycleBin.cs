using Microsoft.EntityFrameworkCore;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
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

    public IReadOnlyList<RecycleBinItem> Get(int appId)
    {
        var l = Log.Fn<IReadOnlyList<RecycleBinItem>>($"appId:{appId}");

        if (appId <= 0)
            return l.Return([], "invalid appId");

        var parentRef = DbVersioning.ParentRefForApp(appId);
        var db = AppWorkCtx.DbStorage.SqlDb;

        const string entitiesTableName = "ToSIC_EAV_Entities";

        var deletedEntities = db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId && e.TransDeletedId != null)
            .Select(e => new
            {
                e.EntityId,
                e.EntityGuid,
                e.AppId,
                e.ContentTypeId,
                e.ContentType,
                DeletedTransactionId = e.TransDeletedId!.Value,
            })
            .ToList();

        // Items which were hard-deleted won't be in TsDynDataEntities anymore.
        // We'll also look in TsDynDataHistory for this app (ParentRef = app-{appId}) and include
        // any entries which don't have a corresponding entity row.
        var entityIdsInApp = db.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == appId)
            .Select(e => e.EntityId);

        var historyMissingEntityRows = db.TsDynDataHistories
            .AsNoTracking()
            .Where(h => h.SourceTable == entitiesTableName
                && h.Operation == EavConstants.HistoryEntityJson
                && h.ParentRef == parentRef
                && h.SourceId != null
                && h.TransactionId != null
                && !entityIdsInApp.Contains(h.SourceId.Value)
            )
            .OrderByDescending(h => h.Timestamp)
            .Select(h => new
            {
                EntityId = h.SourceId!.Value,
                EntityGuid = h.SourceGuid,
                DeletedTransactionId = h.TransactionId!.Value,
                h.Timestamp,
                h.Json,
                h.CJson,
            })
            .ToList();

        var historyDeletedEntitiesLatest = historyMissingEntityRows
            // since we already ordered by Timestamp desc in SQL, First() in each group is the latest
            .GroupBy(h => h.EntityId)
            .Select(g => g.First())
            .ToList();

        var deletedTransactionIds = deletedEntities
            .Select(e => e.DeletedTransactionId)
            .Concat(historyDeletedEntitiesLatest.Select(h => h.DeletedTransactionId))
            .Distinct()
            .ToList();

        var transactions = db.TsDynDataTransactions
            .AsNoTracking()
            .Where(t => deletedTransactionIds.Contains(t.TransactionId))
            .Select(t => new { t.TransactionId, t.Timestamp, t.User })
            .ToList()
            .ToDictionary(t => t.TransactionId);

        var contentTypeIds = deletedEntities
            .Select(e => e.ContentTypeId)
            .Distinct()
            .ToList();

        var contentTypes = db.TsDynDataContentTypes
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(ct => contentTypeIds.Contains(ct.ContentTypeId))
            .Select(ct => new { ct.ContentTypeId, ct.StaticName, ct.Name })
            .ToList()
            .ToDictionary(ct => ct.ContentTypeId);

        var softDeletedItems = deletedEntities
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
                    ParentRef: parentRef
                );
            })
            .ToList();

        var historyOnlyItems = historyDeletedEntitiesLatest
            .Select(h =>
            {
                transactions.TryGetValue(h.DeletedTransactionId, out var tx);

                var json = string.IsNullOrEmpty(h.Json)
                    ? (h.CJson == null ? null : Decompress(h.CJson))
                    : h.Json;

                IEntity? entity = null;
                if (json.HasValue())
                {
                    try
                    {
                        entity = jsonSerializer.New().Deserialize(json);
                    }
                    catch
                    {
                        // ignore parse issues - we'll still return a minimal item
                    }
                }

                return new RecycleBinItem(
                    EntityId: h.EntityId,
                    EntityGuid: h.EntityGuid ?? entity?.EntityGuid ?? Guid.Empty,
                    AppId: appId,
                    ContentTypeStaticName: entity?.Type.NameId ?? "",
                    ContentTypeName: entity?.Type.Name ?? "",
                    DeletedTransactionId: h.DeletedTransactionId,
                    DeletedUtc: tx?.Timestamp ?? h.Timestamp,
                    DeletedBy: tx?.User,
                    ParentRef: parentRef
                );
            })
            // drop entries which we couldn't even identify
            .Where(i => i.EntityGuid != Guid.Empty)
            .ToList();

        var items = softDeletedItems
            .Concat(historyOnlyItems)
            .OrderByDescending(i => i.DeletedUtc)
            .ToList();

        return l.Return(items, $"found:{items.Count}");
    }

    private string? Decompress(byte[] bytes)
        => compressor.Value.Decompress(bytes)
           ?? new Compressor().Decompress(bytes);
}
