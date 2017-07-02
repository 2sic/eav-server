using System;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryManager: ManagerBase
    {
        public QueryManager(AppManager app) : base(app) {}

        public int Clone(int id)
        {
            var eavCt = _appManager.DataController;// DbDataController.Instance(appId: appId);
            var clonedId = new DbPipeline(eavCt).CopyDataPipeline(_appManager.AppId, id, "");// _userName);
            return clonedId;
        }

        public bool Delete(int id)
        {
            //if (_context == null)
            var dbController = _appManager.DataController;//.Instance(appId: appId);

            var canDeleteResult = (dbController.Entities.CanDeleteEntity(id));// _context.EntCommands.CanDeleteEntity(id);
            if (!canDeleteResult.Item1)
                throw new Exception(canDeleteResult.Item2);


            // Get the Entity describing the Pipeline and Pipeline Parts (DataSources)
            // var source = DataSource.GetInitialDataSource(appId: appId);
            var pipelineEntity = DataPipeline.GetPipelineEntity(id, _appManager.Cache);
            var dataSources = DataPipeline.GetPipelineParts(_appManager.ZoneId, _appManager.AppId, pipelineEntity.EntityGuid);
            var metaDataSource = DataSource.GetMetaDataSource(appId: _appManager.AppId);

            // Delete Pipeline Parts
            foreach (var dataSource in dataSources)
            {
                // Delete Configuration Entities (if any)
                var dataSourceConfig = metaDataSource.GetAssignedEntities(Constants.MetadataForEntity /* .AssignmentObjectTypeEntity*/, dataSource.EntityGuid).FirstOrDefault();
                if (dataSourceConfig != null)
                    dbController.Entities.DeleteEntity(dataSourceConfig.EntityId);

                dbController.Entities.DeleteEntity(dataSource.EntityId);
            }

            // Delete Pipeline
            dbController.Entities.DeleteEntity(id);

            return true;

        }
    }
}
