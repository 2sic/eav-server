using System.Collections.Generic;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Persistence.Interfaces;
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
        public AppManager(int zoneId, int appId, Log parentLog = null) : base(zoneId, appId, parentLog) { RenameLog();}

        public AppManager(IApp app, Log parentLog) : base(app, parentLog) { RenameLog();}
        public AppManager(int appId, Log parentLog) : base(appId, parentLog) { RenameLog();}

        private void RenameLog() => Log.Rename("AppMan");

        #endregion

        #region Access the Runtime

        public AppRuntime Read => _runtime ?? (_runtime = new AppRuntime(Data, Log));
        private AppRuntime _runtime;
        #endregion

        internal DbDataController DataController => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId, AppId, Log));
        private DbDataController _eavContext;


        internal IStorage Storage => DataController;

        /// <summary>
        /// The template management subsystem
        /// </summary>
        public TemplatesManager Templates => _templates ?? (_templates = new TemplatesManager(this, Log));
        private TemplatesManager _templates;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _entities ?? (_entities = new EntitiesManager(this, Log));
        private EntitiesManager _entities;

        public QueryManager Queries => _queries ?? (_queries = new QueryManager(this, Log));
        private QueryManager _queries;

        public ContentTypeManager ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeManager(this, Log));
        private ContentTypeManager  _contentTypes;



        public void MetadataEnsureTypeAndSingleEntity(string scope, string setName, string label, int appAssignment, Dictionary<string, object> values)
        {
            ToSicEavAttributeSets contentType;
            if (DataController.AttribSet.DbAttribSetExists(AppId, setName))
                contentType = DataController.AttribSet.GetDbAttribSet(setName);
            else
            {
                contentType = DataController.AttribSet.PrepareDbAttribSet(setName, label, setName, scope, false, null);
                DataController.SqlDb.SaveChanges();
            }

            if (values == null)
                values = new Dictionary<string, object>();

            var newEnt = new Entity(AppId, 0, setName, values);
            newEnt.SetMetadata(new Metadata { KeyNumber = DataController.AppId, TargetType = appAssignment });
            Entities.Save(newEnt);

            SystemManager.Purge(ZoneId, AppId);
        }

    }
}
