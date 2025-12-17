using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils.Compression;

#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal partial class DbVersioning(DbStorage.DbStorage db, LazySvc<Compressor> compressor) : DbPartBase(db, "Db.Version")
{
    private const string EntitiesTableName = "ToSIC_EAV_Entities";

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