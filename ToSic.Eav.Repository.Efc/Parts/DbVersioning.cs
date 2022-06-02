using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public  partial class DbVersioning: BllCommandBase
    {
        private const string EntitiesTableName = "ToSIC_EAV_Entities";

        internal DbVersioning(DbDataController db) : base(db, "Db.Vers") { }

        #region Change-Log ID
        private int _mainChangeLogId;
        /// <summary>
        /// Creates a ChangeLog immediately
        /// </summary>
        /// <remarks>Also opens the SQL Connection to ensure this ChangeLog is used for Auditing on this SQL Connection</remarks>
        internal int GetChangeLogId()
        {
            var userName = DbContext.UserName;
            if (_mainChangeLogId != 0) return _mainChangeLogId;

            var con = DbContext.SqlDb.Database.GetDbConnection();
            if (con.State != ConnectionState.Open)
                con.Open(); // make sure same connection is used later
#if NETFRAMEWORK
            _mainChangeLogId = DbContext.SqlDb.ToSicEavChangeLog
                .FromSql("ToSIC_EAV_ChangeLogAdd @p0", userName)
                .Single().ChangeId;
#else
            // In ef31 FromSqlInterpolated requires SELECT statement in sql string or we get error
            // 'FromSqlRaw or FromSqlInterpolated was called with non-composable SQL and with a query composing over it. Consider calling `AsEnumerable` after the FromSqlRaw or FromSqlInterpolated method to perform the composition on the client side.'.
            // https://github.com/dotnet/efcore/issues/22558#issuecomment-693363140
            // The less worse of all solutions was to copy content from sproc ToSIC_EAV_ChangeLogAdd to sql.
            // FormattableString sql = $"exec ToSIC_EAV_ChangeLogAdd {userName}";
            FormattableString sql = $@"INSERT INTO [dbo].[ToSIC_EAV_ChangeLog] ([Timestamp] ,[User]) VALUES (GetDate(), {userName})
            	DECLARE @ChangeID int
	            SET @ChangeID = scope_identity()
	            EXEC ToSIC_EAV_ChangeLogSet @ChangeID
            	SELECT * FROM [dbo].[ToSIC_EAV_ChangeLog] WHERE [ChangeID] = @ChangeID";
            _mainChangeLogId = DbContext.SqlDb.ToSicEavChangeLog.FromSqlInterpolated(sql).AsEnumerable().Single().ChangeId;
#endif
            return _mainChangeLogId;
        }

#endregion


        public void DoAndSaveHistoryQueue(Action action)
        {
            var callLog = Log.Fn(startTimer: true);
            action.Invoke();
            Save();
            callLog.Done();
        }

        /// <summary>
        /// Save an entity to versioning, which is already serialized
        /// </summary>
        internal void AddToHistoryQueue(int entityId, Guid entityGuid, string serialized)
            => _queue.Add(new ToSicEavDataTimeline
            {
                SourceTable = EntitiesTableName,
                Operation = Constants.DataTimelineEntityJson,
                NewData = "",
                Json = serialized,
                SourceGuid = entityGuid,
                SourceId = entityId,
                SysLogId = GetChangeLogId(),
                SysCreatedDate = DateTime.Now
            });

        /// <summary>
        /// Persist items is queue
        /// </summary>
        private void Save()
        {
            var callLog = Log.Fn(startTimer: true);
            DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.ToSicEavDataTimeline.AddRange(_queue));
            _queue.Clear();
            callLog.Done();
        }

        private readonly List<ToSicEavDataTimeline> _queue = new List<ToSicEavDataTimeline>();
    }
}
