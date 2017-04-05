using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Manage;
using ToSic.Eav.BLL;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: AppBase
    {
        public AppManager(int zoneId, int appId) : base(zoneId, appId) {}

        public AppManager(IApp app) : base(app) {}

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


        internal EavDataController DataController => _eavContext ?? (_eavContext = EavDataController.Instance(ZoneId, AppId));
        private EavDataController _eavContext;


    }
}
