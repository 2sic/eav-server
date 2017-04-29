using System;
using System.Linq;
using Microsoft.Practices.Unity;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Repository.Efc
{

    public class DbDataController
    {
        #region Extracted, now externalized objects with actions and private fields

        public DbShortcuts DbS { get; private set; }

        internal DbVersioning Versioning { get; private set; }
        public DbEntity Entities { get; private set; }
        public DbValue Values { get; private set; }
        public DbAttribute Attributes { get; private set; }
        public DbRelationship Relationships { get; private set; }
        public DbAttributeSet AttribSet { get; private set; }
        internal DbPublishing Publishing { get; private set; }
        public DbDimensions Dimensions { get; private set; }
        public DbZone Zone { get; private set; }
        public DbApp App { get; private set; }
        public DbContentType ContentType { get; private set; }

        public int _appId;
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
                    var uinfo = Factory.Container.Resolve<IEavUserInformation>();
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
            dc.DbS = new DbShortcuts(dc);
            dc.Versioning = new DbVersioning(dc);
            dc.Entities = new DbEntity(dc);
            dc.Values = new DbValue(dc);
            dc.Attributes = new DbAttribute(dc);
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
        /// Get or seth whether SaveChanges() should automatically purge cache.
        /// </summary>
        /// <remarks>Usefull if many changes are made in a batch and Cache should be purged after that batch</remarks>
       public bool PurgeAppCacheOnSave { get; set; } = true;


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

            var modifiedItems = baseEvent(acceptAllChangesOnSuccess);

            if (modifiedItems != 0 && PurgeAppCacheOnSave)
                DataSource.GetCache(ZoneId, AppId).PurgeCache(ZoneId, AppId);

            return modifiedItems;
        }

        #endregion

        #region Shorthand for do & save

        internal void DoAndSave(Action action)
        {
            action.Invoke();
            SqlDb.SaveChanges();
        }
        #endregion
    }
}
