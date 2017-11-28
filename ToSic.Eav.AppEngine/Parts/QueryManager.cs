using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.DataSources.Queries;
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
            var qDef = AppManager.Read.Queries.Get(id);
            var origQuery = qDef.Header;// DataPipeline.GetPipelineEntity(id, AppManager.Cache);
            var newQuery = CopyAndResetIds(origQuery);

            var origParts = qDef.Parts;// DataPipeline.GetPipelineParts(AppManager.ZoneId, AppManager.AppId, origQuery.EntityGuid).ToList();
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


        /// <summary>
        /// Update an existing query in this app
        /// </summary>
        /// <param name="queryId"></param>
        /// <param name="partDefs"></param>
        /// <param name="newDsGuids"></param>
        /// <param name="headerValues"></param>
        /// <param name="wirings"></param>
        public void Update(int queryId, List<Dictionary<string, object>> partDefs, List<Guid> newDsGuids, Dictionary<string, object> headerValues, List<WireInfo> wirings)
        {
            // Get/Save Pipeline EntityGuid. Its required to assign Pipeline Parts to it.
            var qdef = AppManager.Read.Queries.Get(queryId);

            // todo: maybe create a GetBestValue<typed> ? 
            if (((IAttribute<bool?>)qdef.Header["AllowEdit"]).TypedContents == false)
                throw new InvalidOperationException("Pipeline has AllowEdit set to false");

            Dictionary<string, Guid> addedSources = SavePartsAndGenerateRenameMap(
                partDefs, qdef.Header.EntityGuid);

            DeletedRemovedPipelineParts(newDsGuids, addedSources.Values, qdef);

            SaveHeader(queryId, headerValues, wirings, addedSources);
        }

        /// <summary>
        /// Save PipelineParts (DataSources) to EAV
        /// </summary>
        /// <param name="partsDefinitions"></param>
        /// <param name="pipelineEntityGuid">EngityGuid of the Pipeline-Entity</param>
        private Dictionary<string, Guid> SavePartsAndGenerateRenameMap(List<Dictionary<string, object>> partsDefinitions,
            Guid pipelineEntityGuid)
        {
            Log.Add($"save parts guid:{pipelineEntityGuid}");
            var newDataSources = new Dictionary<string, Guid>();

            foreach (var ds in partsDefinitions)
            {
                // go case insensitive...
                var dataSource = new Dictionary<string, object>(ds, StringComparer.InvariantCultureIgnoreCase);
                // Skip Out-DataSource
                var originalIdentity = dataSource[Constants.EntityFieldGuid].ToString();
                dataSource.TryGetValue(Constants.EntityFieldId, out object entityId);

                // remove key-fields, as we cannot save them (would cause error)
                dataSource.Remove(Constants.EntityFieldGuid);
                dataSource.Remove(Constants.EntityFieldId);

                if (originalIdentity == "Out") continue;

                // Update existing DataSource
                if (dataSource.ContainsKey(QueryConstants.VisualDesignerData))
                    dataSource[QueryConstants.VisualDesignerData] = dataSource[QueryConstants.VisualDesignerData].ToString(); // serialize this JSON into string

                if (entityId != null)
                    AppManager.Entities.UpdateParts(Convert.ToInt32(entityId), dataSource);
                // Add new DataSource
                else
                {
                    Tuple<int, Guid> entity = AppManager.Entities.Create(Constants.DataPipelinePartStaticName, dataSource,
                        new MetadataFor { TargetType = Constants.MetadataForEntity, KeyGuid = pipelineEntityGuid });
                    newDataSources.Add(originalIdentity, entity.Item2);
                }
            }

            return newDataSources;
        }
        /// <summary>
        /// Delete Pipeline Parts (DataSources) that are not present
        /// </summary>
        public void DeletedRemovedPipelineParts(
            List<Guid> newEntityGuids, 
            IEnumerable<Guid> newDataSources, 
            QueryDefinition qdef)
        {
            Log.Add($"delete part a#{AppManager.AppId}, pipe:{qdef.Header.EntityGuid}");
            // Get EntityGuids currently stored in EAV
            var existingEntityGuids = qdef.Parts //  DataPipeline.GetPipelineParts(zoneId, appId, pipelineEntityGuid)
                .Select(e => e.EntityGuid);

            // Get EntityGuids from the UI (except Out and unsaved)
            newEntityGuids.AddRange(newDataSources/*.Values*/);

            foreach (var entityToDelete in existingEntityGuids
                .Where(existingGuid => !newEntityGuids.Contains(existingGuid)))
                AppManager.Entities.Delete(entityToDelete);
        }



        /// <summary>
        /// Save a Pipeline Entity to EAV
        /// </summary>
        /// <param name="id">EntityId of the Entity describing the Pipeline</param>
        /// <param name="values"></param>
        /// <param name="wirings"></param>
        /// <param name="renamedDataSources">Array with new DataSources and the unsavedName and final EntityGuid</param>
        /*temp*/ public void SaveHeader(int id, Dictionary<string, object> values, List<WireInfo> wirings, IDictionary<string, Guid> renamedDataSources)
        {
            Log.Add($"save pipe a#{AppManager.AppId}, pipe:{id}");
            wirings = RenameWiring(wirings, renamedDataSources);

            // Validate Stream Wirings, as we should never save bad wirings
            foreach (var wireInfo in wirings.Where(wireInfo => wirings.Count(w => w.To == wireInfo.To && w.In == wireInfo.In) > 1))
                throw new Exception(
                    $"DataSource \"{wireInfo.To}\" has multiple In-Streams with Name \"{wireInfo.In}\". Each In-Stream must have an unique Name and can have only one connection.");

            // add to new object...then send to save/update
            values[Constants.DataPipelineStreamWiringStaticName] = DataPipelineWiring.Serialize(wirings);
            AppManager.Entities.UpdateParts(id, values);
        }

        /// <summary>
        /// Update Wirings of Entities just added - as in the json-text they still have string-names, not the guids
        /// </summary>
        /// <param name="wirings"></param>
        /// <param name="renamedDataSources"></param>
        /// <returns></returns>
        private static List<WireInfo> RenameWiring(List<WireInfo> wirings, IDictionary<string, Guid> renamedDataSources)
        {
            if (renamedDataSources == null) return wirings;

            var wiringsNew = new List<WireInfo>();
            foreach (var wireInfo in wirings)
            {
                var newWireInfo = wireInfo;
                if (renamedDataSources.ContainsKey(wireInfo.From))
                    newWireInfo.From = renamedDataSources[wireInfo.From].ToString();
                if (renamedDataSources.ContainsKey(wireInfo.To))
                    newWireInfo.To = renamedDataSources[wireInfo.To].ToString();
                wiringsNew.Add(newWireInfo);
            }
            return wiringsNew;
        }
    }
}
