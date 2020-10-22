using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
#if NET451
            _mainChangeLogId = DbContext.SqlDb.ToSicEavChangeLog
                .FromSql("ToSIC_EAV_ChangeLogAdd @p1", userName)
                .Single().ChangeId;
#else
            _mainChangeLogId = DbContext.SqlDb.ToSicEavChangeLog
                .FromSqlInterpolated($"exec ToSIC_EAV_ChangeLogAdd {userName}")
                .Single().ChangeId;
#endif
            return _mainChangeLogId;
        }

#endregion


        public void DoAndSaveHistoryQueue(Action action)
        {
            var callLog = Log.Call(useTimer: true);
            action.Invoke();
            Save();
            callLog(null);
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
            var callLog = Log.Call(useTimer: true);
            DbContext.SqlDb.ToSicEavDataTimeline.AddRange(_queue);
            DbContext.SqlDb.SaveChanges();
            _queue.Clear();
            callLog(null);
        }

        private readonly List<ToSicEavDataTimeline> _queue = new List<ToSicEavDataTimeline>();
    }
}
