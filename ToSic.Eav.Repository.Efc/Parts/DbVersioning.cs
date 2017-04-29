using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ObjectBuilder2;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    internal  partial class DbVersioning: BllCommandBase
    {
        internal DbVersioning(DbDataController cntx) : base(cntx)
        {
        }

        private int _mainChangeLogId;

        /// <summary>
        /// This will not version immediately, but wait for another later call to persist the versions
        /// </summary>
        public bool DelayVersioning = false;

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




        /// <summary>
        /// Persist modified Entity to DataTimeline
        /// </summary>
        internal void SaveEntity(int entityId, Guid entityGuid, bool useDelayedSerialize)//ToSicEavEntities currentEntity)
        {
            // if delayed is used, then it should serialize it at the moment the save is done, 
            // an not right now
            // this is important for cases where related entities are added later
            if (useDelayedSerialize)
            {
                QueueForLaterSerialization(entityId, entityGuid);
                return;
            }

            var export = new DbXmlBuilder(DbContext);
            var entityModelSerialized = export.XmlEntity(entityId);
            var timelineItem = new ToSicEavDataTimeline()
            {
                SourceTable = "ToSIC_EAV_Entities",
                Operation = Constants.DataTimelineEntityStateOperation,
                NewData = entityModelSerialized.ToString(),
                SourceGuid = entityGuid,
                SourceId = entityId,
                SysLogId = GetChangeLogId(),
                SysCreatedDate = DateTime.Now
            };
            _timelineQueue.Add(timelineItem);
            if(!DelayVersioning)
                SaveQueue();
            //DbContext.SqlDb.Add(timelineItem);
            //DbContext.SqlDb.SaveChanges();
        }

        public void SaveQueue()
        {
            if (_timelineToDos.Count > 0)
            {
                _timelineToDos.ForEach(td => SaveEntity(td.Key, td.Value, false));
                _timelineToDos.Clear();
            }

            DbContext.SqlDb.ToSicEavDataTimeline.AddRange(_timelineQueue);
            DbContext.SqlDb.SaveChanges();
            _timelineQueue.Clear();
        }

        private void QueueForLaterSerialization(int entityId, Guid entityGuid)
        {
            _timelineToDos[entityId] = entityGuid;
            if(!DelayVersioning)
                SaveQueue();
        }

        private readonly Dictionary<int, Guid> _timelineToDos = new Dictionary<int, Guid>();

        private readonly List<ToSicEavDataTimeline> _timelineQueue = new List<ToSicEavDataTimeline>();

        
    }
}
