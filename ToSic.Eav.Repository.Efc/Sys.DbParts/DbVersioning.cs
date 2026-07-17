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
                // Prepare json package
                var jsonPackage = serializer.ToJson(i.Entity, metadataDepth);

                // Check if we must store any parents-references along with it
                var parents = parentsByChild.TryGetValue(i.EntityId, out var foundParents) && foundParents.Count > 0
                    ? foundParents
                    : null;

                if (parents is { Count: > 0 })
                    jsonPackage = jsonPackage with { Parents = parents };

                // Serialize and save
                var serialized = serializer.Serialize(jsonPackage);
                return PrepareHistoryEntry(i.EntityId, i.EntityGuid, i.ParentRef, serialized);
            })
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
    internal int GetTransactionId()
    {
        var userName = DbStore.UserIdentityToken;
        if (_mainTransactionId != 0)
            return _mainTransactionId;

        var provider = DbStore.SqlDb.Database.ProviderName;
        var (insertSql, identitySql) = provider switch
        {
            "Microsoft.EntityFrameworkCore.SqlServer" =>
                ("INSERT INTO [TsDynDataTransaction] ([Timestamp], [User]) OUTPUT inserted.[TransactionId] VALUES (@timestamp, @userName);", null),
            "Npgsql.EntityFrameworkCore.PostgreSQL" =>
                ("INSERT INTO \"ts_dyn_data_transaction\" (\"timestamp\", \"user\") VALUES (@timestamp, @userName) RETURNING \"transaction_id\";", null),
            "Microsoft.EntityFrameworkCore.Sqlite" =>
                ("INSERT INTO \"TsDynDataTransaction\" (\"Timestamp\", \"User\") VALUES (@timestamp, @userName) RETURNING \"TransactionId\";", null),
            "MySql.EntityFrameworkCore" =>
                ("INSERT INTO `TsDynDataTransaction` (`Timestamp`, `User`) VALUES (@timestamp, @userName);", "SELECT LAST_INSERT_ID();"),
            _ => throw new NotSupportedException($"Unsupported EF Core database provider '{provider}'.")
        };

        var connection = DbStore.SqlDb.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = insertSql;
        var timestamp = command.CreateParameter();
        timestamp.ParameterName = "@timestamp";
        timestamp.Value = DateTime.UtcNow;
        command.Parameters.Add(timestamp);
        var user = command.CreateParameter();
        user.ParameterName = "@userName";
        user.Value = userName ?? (object)DBNull.Value;
        command.Parameters.Add(user);

        var currentTransaction = DbStore.SqlDb.Database.CurrentTransaction?.GetDbTransaction();
        if (currentTransaction != null)
            command.Transaction = currentTransaction;

        object result;
        if (identitySql == null)
            result = command.ExecuteScalar()!;
        else
        {
            command.ExecuteNonQuery();
            command.CommandText = identitySql;
            command.Parameters.Clear();
            result = command.ExecuteScalar()!;
        }

        _mainTransactionId = Convert.ToInt32(result);
        return _mainTransactionId;
    }

    #endregion

    internal static string? ParentRefForApp(int appId)
        => appId > 0 ? $"app-{appId}" : null;

    ///// <summary>
    ///// Save an entity to versioning, which is already serialized
    ///// </summary>

    //internal void AddAndSave(int entityId, Guid entityGuid, string? parentRef, string serialized)
    //    => Save([PrepareHistoryEntry(entityId, entityGuid, parentRef, serialized)]);

    internal TsDynDataHistory PrepareHistoryEntry(int entityId, Guid entityGuid, string? parentRef, string serialized)
        => new()
        {
            SourceTable = EntitiesTableName,
            Operation = EavConstants.HistoryEntityJson,
            Json = compressor.Value.IsEnabled ? null : serialized,
            CJson = compressor.Value.CompressOrNullIfDisabled(serialized),
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
        DbStore.DoAndSaveWithoutChangeDetection(
            () => DbStore.SqlDb.TsDynDataHistories.AddRange(queue)
        );
        l.Done();
    }
}
