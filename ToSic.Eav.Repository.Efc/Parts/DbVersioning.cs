using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using ToSic.Eav.Internal.Compression;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Repository.Efc.Parts;

internal  partial class DbVersioning: DbPartBase
{
    private readonly LazySvc<Compressor> _compressor;
    private const string EntitiesTableName = "ToSIC_EAV_Entities";

    internal DbVersioning(DbDataController db, LazySvc<Compressor> compressor) : base(db, "Db.Vers")
    {
        _compressor = compressor;
    }

    #region Change-Log ID
    private int _mainTransactionId;
    /// <summary>
    /// Creates a TransactionId immediately
    /// </summary>
    /// <remarks>Also opens the SQL Connection to ensure this TransactionId is used for Auditing on this SQL Connection</remarks>
    internal int GetTransactionId()
    {
        var userName = DbContext.UserIdentityToken;
        if (_mainTransactionId != 0) return _mainTransactionId;

        var con = DbContext.SqlDb.Database.GetDbConnection();
        if (con.State != ConnectionState.Open)
            con.Open(); // make sure same connection is used later

        // insert and get TransactionId in one trip – parameterised
        const string sql = "INSERT INTO [dbo].[TsDynDataTransaction] ([Timestamp],[User]) OUTPUT inserted.TransactionId VALUES (GETDATE(), @userName);";

        using var cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new SqlParameter("@userName", userName));

        // enlist in any transaction EF is already using
        var curTx = DbContext.SqlDb.Database.CurrentTransaction?.GetDbTransaction();
        if (curTx != null) cmd.Transaction = curTx;

        _mainTransactionId = Convert.ToInt32(cmd.ExecuteScalar()); // returns the new TransactionId
        return _mainTransactionId;
    }

    #endregion


    public void DoAndSaveHistoryQueue(Action action)
    {
        var l = Log.Fn(timer: true);
        action.Invoke();
        Save();
        l.Done();
    }

    /// <summary>
    /// Save an entity to versioning, which is already serialized
    /// </summary>
    internal void AddToHistoryQueue(int entityId, Guid entityGuid, string serialized)
        => _queue.Add(new()
        {
            SourceTable = EntitiesTableName,
            Operation = EavConstants.HistoryEntityJson,
            Json = _compressor.Value.IsEnabled ? null : serialized,
            CJson = _compressor.Value.Compress(serialized),
            SourceGuid = entityGuid,
            SourceId = entityId,
            TransactionId = GetTransactionId(),
            Timestamp = DateTime.Now
        });

    /// <summary>
    /// Persist items is queue
    /// </summary>
    private void Save()
    {
        var l = Log.Fn(timer: true);
        DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.TsDynDataHistories.AddRange(_queue));
        _queue.Clear();
        l.Done();
    }

    private readonly List<TsDynDataHistory> _queue = [];
}