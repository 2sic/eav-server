using System.Collections.Specialized;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.BLL;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: AppBase
    {
        #region Constructors
        public AppManager(int zoneId, int appId) : base(zoneId, appId) {}

        public AppManager(IApp app) : base(app) {}
        public AppManager(int appId) : base(appId) {}
        #endregion

        #region Access the Runtime

        public AppRuntime Read => _runtime ?? (_runtime = new AppRuntime(Cache));
        private AppRuntime _runtime;
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


        internal DbDataController DataController => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId, AppId));
        private DbDataController _eavContext;


        public void MetadataEnsureTypeAndSingleEntity(string scope, string setName, string label, int appAssignment, OrderedDictionary values)
        {
            var contentType = !DataController.AttribSet.AttributeSetExists(setName, AppId)
                ? DataController.AttribSet.AddContentTypeAndSave(setName, label, setName, scope)
                : DataController.AttribSet.GetAttributeSet(setName);

            if (values == null)
                values = new OrderedDictionary();

            DataController.Entities.AddEntity(contentType, values, /*null,*/ DataController.AppId, appAssignment);

            SystemManager.Purge(ZoneId, AppId);
        }

    }
}
