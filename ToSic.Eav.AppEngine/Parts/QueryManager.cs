using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// query manager to work with queries
    /// </summary>
    public class QueryManager: ManagerBase
    {
        public QueryManager(AppManager app, Log parentLog) : base(app, parentLog, "App.QryMng") {}

        public void Clone(int id)
        {
            // 1. find the currenty query
            var origQuery = DataPipeline.GetPipelineEntity(id, AppManager.Cache);
            var newQuery = CopyAndResetIds(origQuery);

            var origParts = DataPipeline.GetPipelineParts(AppManager.ZoneId, AppManager.AppId, origQuery.EntityGuid).ToList();
            var newParts = origParts.ToDictionary(o => o.EntityGuid, o => CopyAndResetIds(o, newQuery.EntityGuid));

            var metaDataSource = DataSource.GetMetaDataSource(appId: AppManager.AppId);
            var origMetadata = origParts
                .ToDictionary(o => o.EntityGuid, o => metaDataSource.GetMetadata(Constants.MetadataForEntity, o.EntityGuid).FirstOrDefault())
                .Where(m => m.Value != null);

            var newMetadata = origMetadata.Select(o => CopyAndResetIds(o.Value, newParts[o.Key].EntityGuid));

            // now update wiring...
            var origWiring = origQuery.GetBestValue(Constants.DataPipelineStreamWiringStaticName).ToString();
            var keyMap = newParts.ToDictionary(o => o.Key.ToString(), o => o.Value.EntityGuid.ToString());
            var newWiring = RemapWiringToCopy(origWiring, keyMap);

            newQuery.Attributes[Constants.DataPipelineStreamWiringStaticName].Values = new List<IValue>
            {
                ValueBuilder.Build(AttributeTypeEnum.String, newWiring, new List<ILanguage>())
            };

            var saveList = newParts.Select(p => p.Value).Concat(newMetadata).Cast<IEntity>().ToList();
            saveList.Add(newQuery);
            AppManager.Entities.Save(saveList);
        }

        private static string RemapWiringToCopy(string origWiring, Dictionary<string, string> keyMap)
        {
            var wiringsSource = DataPipelineWiring.Deserialize(origWiring);
            var wiringsClone = new List<WireInfo>();
            if (wiringsSource != null)
                foreach (var wireInfo in wiringsSource)
                {
                    var wireInfoClone = wireInfo; // creates a clone of the Struct
                    if (keyMap.ContainsKey(wireInfo.From))
                        wireInfoClone.From = keyMap[wireInfo.From];
                    if (keyMap.ContainsKey(wireInfo.To))
                        wireInfoClone.To = keyMap[wireInfo.To];

                    wiringsClone.Add(wireInfoClone);
                }
            var newWiring = DataPipelineWiring.Serialize(wiringsClone);
            return newWiring;
        }

        private Entity CopyAndResetIds(IEntity origQuery, Guid? newMetadataTarget = null)
        {
            var newSer = Serializer.Serialize(origQuery);
            var newEnt = Serializer.Deserialize(newSer) as Entity;
            newEnt.SetGuid(Guid.NewGuid());
            newEnt.ChangeIdForSaving(0);
            if(newMetadataTarget != null)
                newEnt.Retarget(newMetadataTarget.Value);
            return newEnt;
        }

        private JsonSerializer Serializer 
            => _serializer ?? (_serializer = new JsonSerializer(AppManager.Package, Log));
        private JsonSerializer _serializer;

        public bool Delete(int id)
        {
            var canDeleteResult = AppManager.Entities.CanDelete(id);
            if (!canDeleteResult.Item1)
                throw new Exception(canDeleteResult.Item2);


            // Get the Entity describing the Pipeline and Pipeline Parts (DataSources)
            var pipelineEntity = DataPipeline.GetPipelineEntity(id, AppManager.Cache);
            var parts = DataPipeline.GetPipelineParts(AppManager.ZoneId, AppManager.AppId, pipelineEntity.EntityGuid).ToList();
            var mdSource = DataSource.GetMetaDataSource(appId: AppManager.AppId);

            var mdItems = parts
                .Select(ds => mdSource.GetMetadata(Constants.MetadataForEntity, ds.EntityGuid).FirstOrDefault())
                .Where(md => md != null)
                .Select(md => md.EntityId)
                .ToList();

            // delete in the right order - first the outermost-dependants, then a layer in, and finally the top node
            AppManager.Entities.Delete(mdItems);
            AppManager.Entities.Delete(parts.Select(p => p.EntityId).ToList());
            AppManager.Entities.Delete(id);

            // flush cache
            SystemManager.Purge(AppManager.AppId);

            return true;

        }
    }
}
