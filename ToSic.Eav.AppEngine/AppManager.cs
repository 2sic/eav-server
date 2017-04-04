using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Manage;
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: IApp
    {
        #region basic properties
        /// <summary>
        /// The zone id of this app
        /// </summary>
        public int ZoneId { get; }

        /// <summary>
        /// The app id
        /// </summary>
        public int AppId { get; }
        #endregion


        /// <summary>
        /// The template management subsystem
        /// </summary>
        public TemplatesManager Templates => _templates ?? (_templates = new TemplatesManager(this));
        private TemplatesManager _templates;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _entities ?? (_entities = new EntitiesManager(this));
        private EntitiesManager _entities;

        /// <summary>
        /// Create an app manager for this specific app
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        public AppManager(int zoneId, int appId)
        {
            ZoneId = zoneId;
            AppId = appId;
        }

        public AppManager(IApp app)
        {
            ZoneId = app.ZoneId;
            AppId = app.AppId;
        }

        private EavDataController _eavContext;
        internal EavDataController DataController => _eavContext ?? (_eavContext = EavDataController.Instance(ZoneId, AppId));

        internal ICache Cache => _cache ?? (_cache = DataSource.GetCache(ZoneId, AppId));
        private ICache _cache;
    }
}
