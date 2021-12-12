using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Parts;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Repository.Efc
{

    public class DbDataController : HasLog, IStorage, IAppIdentity
    {

        #region Extracted, now externalized objects with actions and private fields

        public DbVersioning Versioning => _versioning ?? (_versioning = new DbVersioning(this));
        private DbVersioning _versioning;
        public DbEntity Entities => _entities ?? (_entities = new DbEntity(this));
        private DbEntity _entities;
        public DbValue Values => _values ?? (_values = new DbValue(this));
        private DbValue _values;
        public DbAttribute Attributes => _attributes ?? (_attributes = new DbAttribute(this));
        private DbAttribute _attributes;
        public DbRelationship Relationships => _relationships ?? (_relationships = new DbRelationship(this));
        private DbRelationship _relationships;
        public DbAttributeSet AttribSet => _attributeSet ?? (_attributeSet = new DbAttributeSet(this));
        private DbAttributeSet _attributeSet;
        internal DbPublishing Publishing => _publishing ?? (_publishing = new DbPublishing(this));
        private DbPublishing _publishing;
        public DbDimensions Dimensions => _dimensions ?? (_dimensions = new DbDimensions(this));
        private DbDimensions _dimensions;
        public DbZone Zone => _dbZone ?? (_dbZone = new DbZone(this));
        private DbZone _dbZone;
        public DbApp App => _dbApp ?? (_dbApp = new DbApp(this));
        private DbApp _dbApp;
        public DbContentType ContentType => _contentType ?? (_contentType = new DbContentType(this));
        private DbContentType _contentType;

        private int _appId;
        private int _zoneId;
        #endregion

        #region Properties like AppId, ZoneId, UserName etc.

        /// <summary>
        /// AppId of this whole Context
        /// </summary>
        public int AppId => _appId == Constants.AppIdEmpty ? Constants.MetaDataAppId : _appId;

        /// <summary>
        /// ZoneId of this whole Context
        /// </summary>
        public int ZoneId => _zoneId == 0 ? Constants.DefaultZoneId : _zoneId;

        private const string UserNameUnknown = "unresolved(eav)";
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
                    _userName = _userLazy.Value?.IdentityToken ?? UserNameUnknown;
                }
                catch
                {
                    _userName = UserNameUnknown;
                }
                return _userName;
            }
        }

        #endregion

        #region shared logs in case of write commands
        public List<LogItem> ImportLogToBeRefactored { get; }=  new List<LogItem>();

        //public ILog Log { get; private set; }

        #endregion

        #region Constructor and Init

        public DbDataController(
            EavDbContext dbContext, 
            Lazy<Efc11Loader> efcLoaderLazy, 
            Lazy<IUser> userLazy, 
            IServiceProvider serviceProvider,
            IAppsCache appsCache,
            LogHistory logHistory
            ) : base("Db.Data")
        {
            _efcLoaderLazy = efcLoaderLazy;
            _userLazy = userLazy;
            _appsCache = appsCache;
            _logHistory = logHistory;
            SqlDb = dbContext;
            ServiceProvider = serviceProvider;
            SqlDb.AlternateSaveHandler += SaveChanges;
        }

        private readonly Lazy<Efc11Loader> _efcLoaderLazy;
        private readonly Lazy<IUser> _userLazy;
        private readonly IAppsCache _appsCache;
        private readonly LogHistory _logHistory;

        public EavDbContext SqlDb { get; }
        internal IServiceProvider ServiceProvider { get; }



        /// <summary>
        /// Set ZoneId and AppId on current context.
        /// </summary>
        public DbDataController Init(int? zoneId, int? appId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            // If nothing is supplied, use defaults
            if (!zoneId.HasValue && !appId.HasValue)
            {
                _zoneId = Constants.DefaultZoneId;
                _appId = Constants.MetaDataAppId;
                return this;
            }

            // If only AppId is supplied, look up it's zone and use that
            if (!zoneId.HasValue && appId.HasValue)
            {
                var zoneIdOfApp = SqlDb.ToSicEavApps.Where(a => a.AppId == appId.Value).Select(a => (int?)a.ZoneId).SingleOrDefault();
                if (!zoneIdOfApp.HasValue)
                    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", nameof(appId));
                _appId = appId.Value;
                _zoneId = zoneIdOfApp.Value;
                return this;
            }

            // if only ZoneId was supplied, use that...
            _zoneId = zoneId.Value;

            if (appId.HasValue)
            {
                var foundApp = SqlDb.ToSicEavApps.FirstOrDefault(a => a.ZoneId == _zoneId && a.AppId == appId.Value);
                if (foundApp == null)
                    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", nameof(appId));
                _appId = appId.Value;
            }
            else
                _appId = SqlDb.ToSicEavApps.First(a => a.Name == Constants.DefaultAppGuid).AppId;

            return this;
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
            if (_appId == Constants.AppIdEmpty)
                throw new Exception("SaveChanges with AppId 0 not allowed.");

            Versioning.GetChangeLogId();

            var modifiedCount = baseEvent(acceptAllChangesOnSuccess);

            if (modifiedCount != 0)
                PurgeAppCacheIfReady();

            return modifiedCount;
        }

        private void PurgeAppCacheIfReady()
        {
            Log.Call($"{_purgeAppCacheOnSave}")(null);
            if(_purgeAppCacheOnSave) _appsCache.Purge(this);
        }

        #endregion

        #region Shorthand for do & save

        internal void DoAndSave(Action action, string message = null)
        {
            var wrapLog = Log.Call(message: message, useTimer: true);
            action.Invoke();
            SqlDb.SaveChanges();
            wrapLog("completed");
        }


        public void DoInTransaction(Action action)
        {
            var randomId = Guid.NewGuid().ToString().Substring(0, 4);
            var ownTransaction = SqlDb.Database.CurrentTransaction == null ? SqlDb.Database.BeginTransaction() : null;
            var wrapLog = Log.Call($"id:{randomId} - create new trans:{ownTransaction != null}", useTimer: true);
            try
            {
                action.Invoke();
                ownTransaction?.Commit();
                Log.Add($"Transaction {randomId} - completed"); // adds ok to end of block
                wrapLog("transaction ok"); // adds ok to top of block
            }
            catch(Exception e)
            {
                ownTransaction?.Rollback();
                Log.Add("Error: " + e.Message);
                Log.Add($"Transaction {randomId} failed / rollback");
                wrapLog("transaction failed / rollback");
                throw;
            }
        }

        /// <summary>
        /// Get or seth whether SaveChanges() should automatically purge cache.
        /// </summary>
        /// <remarks>Useful if many changes are made in a batch and Cache should be purged after that batch</remarks>
        private bool _purgeAppCacheOnSave = true;

        public void DoButSkipAppCachePurge(Action action)
        {
            var callLog = Log.Call(useTimer: true);
            var before = _purgeAppCacheOnSave;
            _purgeAppCacheOnSave = false;
            action.Invoke();
            _purgeAppCacheOnSave = before;
            callLog(null);
        }

        public void DoWithDelayedCacheInvalidation(Action action)
        {
            var wrapLog = Log.Call(useTimer: true);
            _purgeAppCacheOnSave = false;
            action.Invoke();

            _purgeAppCacheOnSave = true;
            PurgeAppCacheIfReady();
            wrapLog("ok");
        }

        /// <summary>
        /// The loader must use the same connection, to ensure it runs in existing transactions.
        /// Otherwise the loader would be blocked from getting intermediate data while we're running changes. 
        /// </summary>
        public IRepositoryLoader Loader => _loader ?? (_loader = _efcLoaderLazy.Value.UseExistingDb(SqlDb).Init(Log));
        private IRepositoryLoader _loader;

        public void DoWhileQueuingVersioning(Action action) => Versioning.DoAndSaveHistoryQueue(action);

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions)
        {
            var callLog = Log.Call<List<int>>(useTimer: true);
            _logHistory.Add("save-data", Log);
            return callLog("ok", Entities.SaveEntity(entities, saveOptions));
        }

        public void Save(List<IContentType> contentTypes, SaveOptions saveOptions)
            => ContentType.ExtendSaveContentTypes(contentTypes, saveOptions);

        #endregion
    }
}
