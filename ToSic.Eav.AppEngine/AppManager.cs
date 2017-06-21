using System.Collections.Generic;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc;

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

        public AppRuntime Read => _runtime ?? (_runtime = new AppRuntime(Data));
        private AppRuntime _runtime;
        #endregion

        internal DbDataController DataController => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId, AppId));
        private DbDataController _eavContext;


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

        public QueryManager Queries => _queries ?? (_queries = new QueryManager(this));
        private QueryManager _queries;




        public void MetadataEnsureTypeAndSingleEntity(string scope, string setName, string label, int appAssignment, Dictionary<string, object> values)
        {
            ToSicEavAttributeSets contentType;
            if (DataController.AttribSet.AttributeSetExists(AppId, setName))
                contentType = DataController.AttribSet.GetAttributeSet(setName);
            else
            {
                contentType = DataController.AttribSet.PrepareSet(setName, label, setName, scope, /*true,*/ false, null);
                DataController.SqlDb.SaveChanges();
            }

            if (values == null)
                values = new Dictionary<string, object>();

            var newEnt = new Entity(0, setName, values);
            newEnt.SetMetadata(new Metadata { KeyNumber = DataController.AppId, TargetType = appAssignment });
            DataController.Entities.SaveEntity(newEnt, new SaveOptions());

            //DataController.Entities.AddEntity(contentType.AttributeSetId, values, new Metadata { KeyNumber = DataController.AppId, TargetType = appAssignment});

            SystemManager.Purge(ZoneId, AppId);
        }

    }
}
