using System;
using System.Linq;
using ToSic.Eav.BLL.Parts;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.BLL
{

    public class EavDataController
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
        internal DbApp App { get; private set; }
        public DbContentType ContentType { get; private set; }

        public RepositoryEF4 Repository { get; private set; }

        public int _appId;
        internal int _zoneId;
        #endregion

        #region Properties like AppId, ZoneId, UserName etc.

        // todo: remove the get/set because the default is already set in initialization...


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

        /// <summary>
        /// Current UserName. Used for ChangeLog
        /// </summary>
        public string UserName { get; set; }

        #endregion
        
        
        #region new stuff

        public EavContext SqlDb { get; private set; }

        private EavDataController()
        {
        }


        #endregion

        #region Constructor and Init
        /// <summary>
        /// Returns a new instace of the Eav Context. InitZoneApp must be called afterward.
        /// </summary>
        private static EavDataController Instance()
        {
            var connectionString = Configuration.GetConnectionString();
            var context = new EavContext(connectionString);
            var dc = new EavDataController {SqlDb = context};
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

            dc.Repository = new RepositoryEF4(dc);

            dc.SqlDb.AlternateSaveHandler += dc.SaveChanges;

            return dc;
        }

        /// <summary>
        /// Returns a new instace of the Eav Context on specified ZoneId and/or AppId
        /// </summary>
        public static EavDataController Instance(int? zoneId = null, int? appId = null)
        {
            var context = Instance();
            context.InitZoneApp(zoneId, appId);

            return context;
        }

        /// <summary>
        /// Set ZoneId and AppId on current context.
        /// </summary>
        public void InitZoneApp(int? zoneId = null, int? appId = null)
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
                var zoneIdOfApp = SqlDb.Apps.Where(a => a.AppID == appId.Value).Select(a => (int?)a.ZoneID).SingleOrDefault();
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
                var foundApp = SqlDb.Apps.FirstOrDefault(a => a.ZoneID == _zoneId && a.AppID == appId.Value);
                if (foundApp == null)
                    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", nameof(appId));
                _appId = appId.Value;
            }
            else
                //_appId = zone.Apps.Where(a => a.Value == Constants.DefaultAppName).Select(a => a.Key).Single();
                _appId = SqlDb.Apps.First(a => a.Name == Constants.DefaultAppName).AppID;

            #region code before refactoring 2015-08-12 - 2dm wanted to get rid of depenedncy on the the DataSource - Cache

            //// If nothing is supplied, use defaults
            //if (!zoneId.HasValue && !appId.HasValue)
            //{
            //    _zoneId = Constants.DefaultZoneId;
            //    _appId = Constants.MetaDataAppId;
            //    return;
            //}

            //// If only AppId is supplied, look up it's zone and use that
            //if (!zoneId.HasValue && appId.HasValue)
            //{
            //    var zoneIdOfApp = Apps.Where(a => a.AppID == appId.Value).Select(a => (int?)a.ZoneID).SingleOrDefault();
            //    if (!zoneIdOfApp.HasValue)
            //        throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
            //    _appId = appId.Value;
            //    _zoneId = zoneIdOfApp.Value;
            //    return;
            //}

            //// if only ZoneId was supplied, use that...
            //_zoneId = zoneId.Value;

            //// ...and try to find the best match for App-ID
            //var zone = ((DataSources.Caches.BaseCache)DataSource.GetCache(_zoneId, null)).ZoneApps[_zoneId];

            //if (appId.HasValue)
            //{
            //    // Set AppId and validate AppId exists with specified ZoneId
            //    //var foundAppId = zone.Apps.Where(a => a.Key == appId.Value).Select(a => (int?)a.Key).SingleOrDefault();
            //    //if (!foundAppId.HasValue)
            //    //    throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
            //    //_appId = foundAppId.Value;
            //    var foundApp = Apps.FirstOrDefault(a => a.ZoneID == _zoneId && a.AppID == appId.Value);
            //    if (foundApp == null)
            //        throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", "appId");
            //    _appId = foundApp.AppID;
            //}
            //else
            //    _appId = zone.Apps.Where(a => a.Value == Constants.DefaultAppName).Select(a => a.Key).Single();

            #endregion
        }

        #endregion

        #region Save and check if to kill cache

        /// <summary>
        /// Get or seth whether SaveChanges() should automatically purge cache.
        /// </summary>
        /// <remarks>Usefull if many changes are made in a batch and Cache should be purged after that batch</remarks>
        private bool _purgeAppCacheOnSave = true;

        public bool PurgeAppCacheOnSave
        {
            get { return _purgeAppCacheOnSave; }
            set { _purgeAppCacheOnSave = value; }
        }



        /// <summary>
        /// Persists all updates to the data source and optionally resets change tracking in the object context.
        /// Also Creates an initial ChangeLog (used by SQL Server for Auditing).
        /// If items were modified, Cache is purged on current Zone/App
        /// </summary>
        public int SaveChanges(System.Data.Objects.SaveOptions options, EavContext.OriginalSaveChangesEvent baseEvent)
        {
            if (_appId == 0)
                throw new Exception("SaveChanges with AppId 0 not allowed.");

            // enure changelog exists and is set to SQL CONTEXT_INFO variable
            if (Versioning.MainChangeLogId == 0)
                Versioning.GetChangeLogId(UserName);

            var modifiedItems = baseEvent(options);

            if (modifiedItems != 0 && PurgeAppCacheOnSave)
                DataSource.GetCache(ZoneId, AppId).PurgeCache(ZoneId, AppId);

            return modifiedItems;
        }

        #endregion

    }
}
