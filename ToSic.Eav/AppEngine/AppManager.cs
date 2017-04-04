using ToSic.Eav.BLL;

namespace ToSic.Eav.AppEngine
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager
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

        private TemplateManager _templates;

        /// <summary>
        /// The template management subsystem
        /// </summary>
        public TemplateManager Templates => _templates ?? (_templates = new TemplateManager(this));

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

        private EavDataController _eavContext;
        internal EavDataController DataController => _eavContext ?? (_eavContext = EavDataController.Instance(ZoneId, AppId));

    }
}
