using System;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.Practices.Unity;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Repository.Efc
{

    public class DbDataController : IStorage
    {
        #region Extracted, now externalized objects with actions and private fields

        public DbVersioning Versioning { get; private set; }
        public DbEntity Entities { get; private set; }
        public DbValue Values { get; private set; }
        public DbAttributeDefinition AttributesDefinition { get; private set; }
        public DbRelationship Relationships { get; private set; }
        public DbAttributeSet AttribSet { get; private set; }
        internal DbPublishing Publishing { get; private set; }
        public DbDimensions Dimensions { get; private set; }
        public DbZone Zone { get; private set; }
        public DbApp App { get; private set; }
        public DbContentType ContentType { get; private set; }

        private int _appId;
        internal int _zoneId;
        #endregion

        #region Properties like AppId, ZoneId, UserName etc.

        /// <summary>
        /// AppId of this whole Context
        /// </summary>
        public int AppId
        {
            get { return _appId == 0 ? Constants.MetaDataAppId : _appId; }
            set { _appId = value; }
        }

        /// <summary>
        /// ZoneId of this whole Context
        /// </summary>
        public int ZoneId
        {
            get { return _zoneId == 0 ? Constants.DefaultZoneId : _zoneId; }
            set { _zoneId = value; }
        }

        private string _userName;
        /// <summary>
        /// Current UserName. Used for ChangeLog
        /// </summary>
        public string UserName {
            get
            {
                if (_userName != null) return _userName;
                try
                {
                    // try to get using dependency injection
                    var uinfo = Factory.Resolve<IEavUserInformation>();
                    _userName = uinfo.IdentityForLog;
                }
                catch
                {
                    _userName = "unresolved(eav)";
                }
                return _userName;
            }
            set
            {
                _userName = value;
            }}

        #endregion

        #region shared logs in case of write commands
        public List<LogItem> Log { get; }=  new List<LogItem>();
        #endregion

        #region new stuff

        public EavDbContext SqlDb { get; private set; }

        private DbDataController()
        {
        }


        #endregion


        #region Constructor and Init

        /// <summary>
        /// Returns a new instace of the Eav Context. InitZoneApp must be called afterward.
        /// </summary>
        private static DbDataController Instance()
        {
            var context = Factory.Resolve<EavDbContext>();
            var dc = new DbDataController {SqlDb = context};
            dc.Versioning = new DbVersioning(dc);
            dc.Entities = new DbEntity(dc);
            dc.Values = new DbValue(dc);
            dc.AttributesDefinition = new DbAttributeDefinition(dc);
            dc.Relationships = new DbRelationship(dc);
            dc.AttribSet = new DbAttributeSet(dc);
            dc.Publishing = new DbPublishing(dc);
            dc.Dimensions = new DbDimensions(dc);
            dc.Zone = new DbZone(dc);
            dc.App = new DbApp(dc);
            dc.ContentType = new DbContentType(dc);

            dc.SqlDb.AlternateSaveHandler += dc.SaveChanges;

            return dc;
        }

        /// <summary>
        /// Returns a new instace of the Eav Context on specified ZoneId and/or AppId
        /// </summary>
        public static DbDataController Instance(int? zoneId = null, int? appId = null)
        {
            var context = Instance();
            context.InitZoneApp(zoneId, appId);

            return context;
        }

        /// <summary>
        /// Set ZoneId and AppId on current context.
        /// </summary>
        private void InitZoneApp(int? zoneId = null, int? appId = null)
        {
            // If nothing is supplied, use defaults
            if (!zoneId.HasValue && !appId.HasValue)
            {
                _zoneId = Constants.DefaultZoneId;
                _appId = Constants.MetaDataAppId;
                return;
            }

            // If only AppId is supplied, look up it's zone and use that
            if (!zoneId.HasValue && appId.HasValue)
            {
                var zoneIdOfApp = SqlDb.ToSicEavApps.Where(a => a.AppId == appId.Value).Select(a => (int?)a.ZoneId).SingleOrDefault();
                if (!zoneIdOfApp.HasValue)
                    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", nameof(appId));
                _appId = appId.Value;
                _zoneId = zoneIdOfApp.Value;
                return;
            }

            // if only ZoneId was supplied, use that...
            _zoneId = zoneId.Value;

            // ...and try to find the best match for App-ID
            // var zone = ((DataSources.Caches.BaseCache)DataSource.GetCache(_zoneId, null)).ZoneApps[_zoneId];

            if (appId.HasValue)
            {
                var foundApp = SqlDb.ToSicEavApps.FirstOrDefault(a => a.ZoneId == _zoneId && a.AppId == appId.Value);
                if (foundApp == null)
                    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", nameof(appId));
                _appId = appId.Value;
            }
            else
                _appId = SqlDb.ToSicEavApps.First(a => a.Name == Constants.DefaultAppName).AppId;

        }

        #endregion

        #region Save and check if to kill cache



        /// <summary>
        /// Persists all updates to the data source and optionally resets change tracking in the object context.
        /// Also Creates an initial ChangeLog (used by SQL Server for Auditing).
        /// If items were modified, Cache is purged on current Zone/App
        /// </summary>
        public int SaveChanges(bool acceptAllChangesOnSuccess, EavDbContext.SaveChangesEvent baseEvent)
        {
            if (_appId == 0)
                throw new Exception("SaveChanges with AppId 0 not allowed.");

            // enure changelog exists and is set to SQL CONTEXT_INFO variable
            //if (Versioning.MainChangeLogId == 0)
            Versioning.GetChangeLogId();

            var modifiedCount = baseEvent(acceptAllChangesOnSuccess);


            if (modifiedCount != 0) // && PurgeAppCacheOnSave)
                PurgeAppCacheIfReady();
                //(_cache ?? (_cache = Factory.Resolve<ICache>())).PurgeCache(ZoneId, AppId);
                //DataSource.GetCache(ZoneId, AppId).PurgeCache(ZoneId, AppId);

            return modifiedCount;
        }

        private void PurgeAppCacheIfReady()
        {
            if(_purgeAppCacheOnSave)
                (_cache ?? (_cache = Factory.Resolve<ICache>())).PurgeCache(ZoneId, AppId);
        }

        private ICache _cache;

        #endregion

        #region Shorthand for do & save

        internal void DoAndSave(Action action)
        {
            action.Invoke();
            SqlDb.SaveChanges();
        }


        public void DoInTransaction(Action action)
        {
            var ownTransaction = SqlDb.Database.CurrentTransaction == null ? SqlDb.Database.BeginTransaction() : null;

            try
            {
                action.Invoke();
                ownTransaction?.Commit();
            }
            catch
            {
                ownTransaction?.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Get or seth whether SaveChanges() should automatically purge cache.
        /// </summary>
        /// <remarks>Usefull if many changes are made in a batch and Cache should be purged after that batch</remarks>
        private bool _purgeAppCacheOnSave = true;

        public void DoWithDelayedCacheInvalidation(Action action)
        {
            _purgeAppCacheOnSave = false;
            action.Invoke();

            _purgeAppCacheOnSave = true;
            PurgeAppCacheIfReady();
        }

        public IRepositoryLoader Loader => new Efc11Loader(SqlDb);
        public void DoWhileQueuingVersioning(Action action) => Versioning.QueueDuringAction(action);
        public void DoWhileQueueingRelationships(Action action) => Relationships.DoWhileQueueingRelationships(action);

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions) => Entities.SaveEntity(entities, saveOptions);

        public int Save(IEntity entity, SaveOptions saveOptions) => Entities.SaveEntity(entity, saveOptions);
        public void Save(List<IContentType> contentTypes, SaveOptions saveOptions) => ContentType.ExtendSaveContentTypes(contentTypes, saveOptions /* SaveOptions.Build(ZoneId)*/);

        #endregion
    }
}
