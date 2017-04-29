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
        internal DbVersioning(DbDataController cntx) : base(cntx) { }

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

        public void ActivateQueue() => _useQueue = true;

        public void ProcessQueue()
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
                SerializeEntityAndAddToQueue(entityId, entityGuid);

            if(!_useQueue)
                Save();
        }

        /// <summary>
        /// Convert an entity to xml and add to saving queue
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityGuid"></param>
        private void SerializeEntityAndAddToQueue(int entityId, Guid entityGuid)
        {
            var export = new DbXmlBuilder(DbContext);
            var entityModelSerialized = export.XmlEntity(entityId);
            var timelineItem = new ToSicEavDataTimeline
            {
                SourceTable = "ToSIC_EAV_Entities",
                Operation = Constants.DataTimelineEntityStateOperation,
                NewData = entityModelSerialized.ToString(),
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
            // now handle the delayed queue, which waited with serializing
            _delaySerialization.ForEach(td => SerializeEntityAndAddToQueue(td.Key, td.Value));
            _delaySerialization.Clear();

            DbContext.SqlDb.ToSicEavDataTimeline.AddRange(_queue);
            DbContext.SqlDb.SaveChanges();
            _queue.Clear();
        }


        private readonly Dictionary<int, Guid> _delaySerialization = new Dictionary<int, Guid>();

        private readonly List<ToSicEavDataTimeline> _queue = new List<ToSicEavDataTimeline>();

        
    }
}
