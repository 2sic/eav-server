using System;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// query manager to work with queries
    /// </summary>
    public class QueryManager: ManagerBase
    {
        public QueryManager(AppManager app, Log parentLog) : base(app, parentLog, "App.QryMng") {}

        public int Clone(int id)
        {
            var eavCt = AppManager.DataController;// DbDataController.Instance(appId: appId);
            var clonedId = new DbPipeline(eavCt).CopyDataPipeline(AppManager.AppId, id, "");// _userName);
            return clonedId;
        }

        public bool Delete(int id)
        {
            //if (_context == null)
            var dbController = AppManager.DataController;//.Instance(appId: appId);

            var canDeleteResult = (dbController.Entities.CanDeleteEntity(id));// _context.EntCommands.CanDeleteEntity(id);
            if (!canDeleteResult.Item1)
                throw new Exception(canDeleteResult.Item2);


            // Get the Entity describing the Pipeline and Pipeline Parts (DataSources)
            // var source = DataSource.GetInitialDataSource(appId: appId);
            var pipelineEntity = DataPipeline.GetPipelineEntity(id, AppManager.Cache);
            var dataSources = DataPipeline.GetPipelineParts(AppManager.ZoneId, AppManager.AppId, pipelineEntity.EntityGuid);
            var metaDataSource = DataSource.GetMetaDataSource(appId: AppManager.AppId);

            // Delete Pipeline Parts
            foreach (var dataSource in dataSources)
            {
                // Delete Configuration Entities (if any)
                var dataSourceConfig = metaDataSource.GetMdGeneric(Constants.MetadataForEntity /* .AssignmentObjectTypeEntity*/, dataSource.EntityGuid).FirstOrDefault();
                if (dataSourceConfig != null)
                    dbController.Entities.DeleteEntity(dataSourceConfig.EntityId);

                dbController.Entities.DeleteEntity(dataSource.EntityId);
            }

            // Delete Pipeline
            dbController.Entities.DeleteEntity(id);

            // flush cache
            SystemManager.Purge(AppManager.AppId);

            return true;

        }
    }
}
