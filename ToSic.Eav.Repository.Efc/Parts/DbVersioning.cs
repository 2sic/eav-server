using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
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


        /// <summary>
        /// This will not version immediately, but wait for another later call to persist the versions
        /// </summary>
        private bool _useQueue;


        public void QueueDuringAction(Action action)
        {
            _useQueue = true;
            action.Invoke();
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            _useQueue = false;
            Save();
        }

        /// <summary>
        /// Persist modified Entity to DataTimeline
        /// </summary>
        internal void SaveEntity(int entityId, Guid entityGuid, bool useDelayedSerialize)
        {
            // if delayed is used, then it should serialize it at the moment the save is done, 
            // an not right now - so it should only queue the IDs, not the generated XML
            // this is important for cases where related entities are added later
            if (useDelayedSerialize)
                _delaySerialization[entityId] = entityGuid;
            else
                SerializeEntityAndAddToQueue(ImmediateStateSerializer(), entityId, entityGuid);

            if(!_useQueue)
                Save();
        }

        private IThingSerializer ImmediateStateSerializer()
        {
            var serializer = new JsonSerializer();
            var loader = new Efc11Loader(DbContext.SqlDb);
            var appPackageRightNow = loader.AppPackage(DbContext.AppId, parentLog:Log);
            serializer.Initialize(appPackageRightNow);
            return serializer;
        }

        /// <summary>
        /// Convert an entity to xml and add to saving queue
        /// </summary>
        private void SerializeEntityAndAddToQueue(IThingSerializer serializer, int entityId, Guid entityGuid)
        {
            var entityModelSerialized = serializer.Serialize(entityId);
            var timelineItem = new ToSicEavDataTimeline
            {
                SourceTable = EntitiesTableName, Operation = Constants.DataTimelineEntityJson,
                NewData = "",
                Json=entityModelSerialized,
                SourceGuid = entityGuid,
                SourceId = entityId,
                SysLogId = GetChangeLogId(),
                SysCreatedDate = DateTime.Now
            };
            _queue.Add(timelineItem);
        }

        /// <summary>
        /// Persist items is queue
        /// </summary>
        private void Save()
        {
            var sharedSerializer = ImmediateStateSerializer();
            // now handle the delayed queue, which waited with serializing
            _delaySerialization.ToList().ForEach(td => SerializeEntityAndAddToQueue(sharedSerializer, td.Key, td.Value));
            _delaySerialization.Clear();

            DbContext.SqlDb.ToSicEavDataTimeline.AddRange(_queue);
            DbContext.SqlDb.SaveChanges();
            _queue.Clear();
        }


        private readonly Dictionary<int, Guid> _delaySerialization = new Dictionary<int, Guid>();

        private readonly List<ToSicEavDataTimeline> _queue = new List<ToSicEavDataTimeline>();

        
    }
}
