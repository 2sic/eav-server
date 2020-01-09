using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Serialization;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public  partial class DbVersioning: BllCommandBase
    {
        private const string EntitiesTableName = "ToSIC_EAV_Entities";

        internal DbVersioning(DbDataController cntx) : base(cntx, "Db.Vers") { }

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
                con.Open();	// make sure same connection is used later

            _mainChangeLogId = DbContext.SqlDb.ToSicEavChangeLog
                // ReSharper disable once FormatStringProblem
                .FromSql("ToSIC_EAV_ChangeLogAdd @p0", userName)
                .Single().ChangeId;

            return _mainChangeLogId;
        }

        #endregion


        public void QueueDuringAction(Action action)
        {
            action.Invoke();
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            Save();
        }

        private IDataSerializer ImmediateStateSerializer()
        {
            var loader = new Efc11Loader(DbContext.SqlDb);
            var appPackageRightNow = loader.AppState(DbContext.AppId, parentLog:Log);
            var serializer = new JsonSerializer(appPackageRightNow, Log);
            return serializer;
        }

        /// <summary>
        /// Convert an entity to xml and add to saving queue
        /// </summary>
        private void SerializeEntityAndAddToQueue(IDataSerializer serializer, int entityId, Guid entityGuid) 
            => SaveEntity(entityId, entityGuid, serializer.Serialize(entityId));

        /// <summary>
        /// Save an entity to versioning, which is already serialized
        /// </summary>
        public void SaveEntity(int entityId, Guid entityGuid, string serialized)
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
            DbContext.SqlDb.ToSicEavDataTimeline.AddRange(_queue);
            DbContext.SqlDb.SaveChanges();
            _queue.Clear();
        }

        private readonly List<ToSicEavDataTimeline> _queue = new List<ToSicEavDataTimeline>();

        
    }
}
