#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils.Compression;

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal partial class DbVersioning(DbStorage.DbStorage db, LazySvc<Compressor> compressor) : DbPartBase(db, "Db.Version")
{
    private const string EntitiesTableName = "ToSIC_EAV_Entities";

    internal List<TsDynDataHistory> PrepareHistoryEntriesWithInboundParents(
        IReadOnlyCollection<(IEntity Entity, int EntityId, Guid EntityGuid, string? ParentRef)> items,
        int metadataDepth = 0)
    {
        var l = LogDetails.Fn<List<TsDynDataHistory>>(timer: true);
        if (items.Count == 0)
            return l.Return([]);

        var ids = items
            .Select(i => i.EntityId)
            .Distinct()
            .ToList();

        var parentsByChild = GetInboundParentsByChildIds(ids);
        var serializer = DbStore.JsonSerializerGenerator.New();

        var historyEntries = items
            .Select(i =>
            {
                var parents = parentsByChild.TryGetValue(i.EntityId, out var foundParents) && foundParents.Count > 0
                    ? foundParents
                    : null;

                var serialized = serializer.Serialize(i.Entity, parents, metadataDepth);
                return PrepareHistoryEntry(i.EntityId, i.EntityGuid, i.ParentRef, serialized);
            })
            .Where(h => h != null)
            .ToList();

        return l.ReturnAsOk(historyEntries);
    }

    internal void AddAndSave(IEntity entity, int entityId, Guid entityGuid, string? parentRef)
    {
        var entries = PrepareHistoryEntriesWithInboundParents([(entity, entityId, entityGuid, parentRef)]);
        Save(entries);
    }

    private Dictionary<int, List<JsonRelationship>> GetInboundParentsByChildIds(IReadOnlyCollection<int> entityIds)
    {
        if (entityIds.Count == 0)
            return [];

        // Note: Relationship has a global query filter for TransDeletedId == null.
        // We want only active inbound relations at the time the history snapshot is created.
        var inbound = DbStore.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Where(r => r.ChildEntityId != null && entityIds.Contains(r.ChildEntityId.Value))
            .Select(r => new
            {
                ChildId = r.ChildEntityId!.Value,
                ParentGuid = r.ParentEntity.EntityGuid,
                Field = r.Attribute.StaticName,
                r.SortOrder,
            })
            .ToList();

        return inbound
            .GroupBy(r => r.ChildId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(r => new JsonRelationship
                    {
                        Parent = r.ParentGuid,
                        Field = r.Field,
                        SortOrder = r.SortOrder
                    })
                    .ToList()
            );
    }

    #region Change-Log ID

    private int _mainTransactionId;

    /// <summary>
    /// Creates a TransactionId immediately
    /// </summary>
    /// <remarks>Also opens the SQL Connection to ensure this TransactionId is used for Auditing on this SQL Connection</remarks>
    internal int GetTransactionId()
    {
        var userName = DbStore.UserIdentityToken;
        if (_mainTransactionId != 0)
            return _mainTransactionId;

        var con = DbStore.SqlDb.Database.GetDbConnection();
        if (con.State != ConnectionState.Open)
            con.Open(); // make sure same connection is used later

        // insert and get TransactionId in one trip – parameterised
        const string sql = "INSERT INTO [dbo].[TsDynDataTransaction] ([Timestamp],[User]) OUTPUT inserted.TransactionId VALUES (GETUTCDATE(), @userName);";

        using var cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new SqlParameter("@userName", userName));

        // enlist in any transaction EF is already using
        var curTx = DbStore.SqlDb.Database.CurrentTransaction?.GetDbTransaction();
        if (curTx != null)
            cmd.Transaction = curTx;

        _mainTransactionId = Convert.ToInt32(cmd.ExecuteScalar()); // returns the new TransactionId
        return _mainTransactionId;
    }

    #endregion

    internal static string? ParentRefForApp(int appId)
        => appId > 0 ? $"app-{appId}" : null;

    /// <summary>
    /// Save an entity to versioning, which is already serialized
    /// </summary>

    internal void AddAndSave(int entityId, Guid entityGuid, string? parentRef, string serialized)
        => Save([PrepareHistoryEntry(entityId, entityGuid, parentRef, serialized)]);

    internal TsDynDataHistory PrepareHistoryEntry(int entityId, Guid entityGuid, string? parentRef, string serialized)
        => new()
        {
            SourceTable = EntitiesTableName,
            Operation = EavConstants.HistoryEntityJson,
            Json = compressor.Value.IsEnabled ? null : serialized,
            CJson = compressor.Value.Compress(serialized),
            SourceGuid = entityGuid,
            SourceId = entityId,
            ParentRef = parentRef,
            TransactionId = GetTransactionId(),
            Timestamp = DateTime.UtcNow // always UTC (time zone independent)
        };

    /// <summary>
    /// Persist items is queue
    /// </summary>
    /// <param name="queue"></param>
    internal void Save(ICollection<TsDynDataHistory> queue)
    {
        var l = LogDetails.Fn(timer: true);
        DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.TsDynDataHistories.AddRange(queue));
        l.Done();
    }
}