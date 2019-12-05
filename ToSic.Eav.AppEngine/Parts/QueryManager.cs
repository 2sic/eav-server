using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// query manager to work with queries
    /// </summary>
    public class QueryManager: ManagerBase
    {
        public QueryManager(AppManager app, ILog parentLog) : base(app, parentLog, "App.QryMng") {}

        public void SaveCopy(int id) => SaveCopy(AppManager.Read.Queries.Get(id));

        public void SaveCopy(QueryDefinition query)
        {
            var newQuery = CopyAndResetIds(query.Entity);
            var newParts = query.Parts.ToDictionary(o => o.Guid, o => CopyAndResetIds(o.Entity, newQuery.EntityGuid));

            var origMetadata = query.Parts
                .ToDictionary(o => o.Guid, o => o.Entity.Metadata.FirstOrDefault())
                .Where(m => m.Value != null);

            var newMetadata = origMetadata.Select(o => CopyAndResetIds(o.Value, newParts[o.Key].EntityGuid));

            // now update wiring...
            var origWiring = query.Connections;// query.Entity.GetBestValue(Constants.QueryStreamWiringAttributeName).ToString();
            var keyMap = newParts.ToDictionary(o => o.Key.ToString(), o => o.Value.EntityGuid.ToString());
            var newWiring = RemapWiringToCopy(origWiring, keyMap);

            newQuery.Attributes[Constants.QueryStreamWiringAttributeName].Values = new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, newWiring, new List<ILanguage>())
            };

            var saveList = newParts.Select(p => p.Value).Concat(newMetadata).Cast<IEntity>().ToList();
            saveList.Add(newQuery);
            AppManager.Entities.Save(saveList);
        }

        private static string RemapWiringToCopy(IList<Connection> origWiring, Dictionary<string, string> keyMap)
        {
            var wiringsSource = origWiring;// QueryWiring.Deserialize(origWiring);
            var wiringsClone = new List<Connection>();
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
            var newWiring = Connections.Serialize(wiringsClone);
            return newWiring;
        }

        private Entity CopyAndResetIds(IEntity origQuery, Guid? newMetadataTarget = null)
        {
            var newSer = Serializer.Serialize(origQuery);
            var newEnt = Serializer.Deserialize(newSer) as Entity;
            newEnt.SetGuid(Guid.NewGuid());
            newEnt.ResetEntityId(0);
            if(newMetadataTarget != null)
                newEnt.Retarget(newMetadataTarget.Value);
            return newEnt;
        }

        private JsonSerializer Serializer 
            => _serializer ?? (_serializer = new JsonSerializer(AppManager.Package, Log));
        private JsonSerializer _serializer;

        public bool Delete(int id)
        {
            Log.Add($"delete a#{AppManager.AppId}, id:{id}");
            var canDeleteResult = AppManager.Entities.CanDelete(id);
            if (!canDeleteResult.Item1)
                throw new Exception(canDeleteResult.Item2);


            // Get the Entity describing the Query and Query Parts (DataSources)
            var queryEntity = Eav.DataSources.Queries.QueryManager.GetQueryEntity(id, AppManager.Cache);
            var qDef = new QueryDefinition(queryEntity, AppManager.AppId);

            var mdItems = qDef.Parts// parts
                .Select(ds => ds.Entity.Metadata.FirstOrDefault())
                .Where(md => md != null)
                .Select(md => md.EntityId)
                .ToList();

            // delete in the right order - first the outermost-dependents, then a layer in, and finally the top node
            AppManager.Entities.Delete(mdItems);
            AppManager.Entities.Delete(qDef.Parts.Select(p => p.Id).ToList());
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
        public void Update(int queryId, List<Dictionary<string, object>> partDefs, List<Guid> newDsGuids, Dictionary<string, object> headerValues, List<Connection> wirings)
        {
            // Get/Save Query EntityGuid. Its required to assign Query Parts to it.
            var qdef = AppManager.Read.Queries.Get(queryId);

            // todo: maybe create a GetBestValue<typed> ? 
            if (((IAttribute<bool?>)qdef.Entity["AllowEdit"]).TypedContents == false)
                throw new InvalidOperationException("Query has AllowEdit set to false");

            Dictionary<string, Guid> addedSources = SavePartsAndGenerateRenameMap(
                partDefs, qdef.Entity.EntityGuid);

            DeletedRemovedParts(newDsGuids, addedSources.Values, qdef);

            headerValues = new Dictionary<string, object>(headerValues, StringComparer.InvariantCultureIgnoreCase);
            RemoveIdAndGuidFromValues(headerValues);
            SaveHeader(queryId, headerValues, wirings, addedSources);
        }

        /// <summary>
        /// Save QueryParts (DataSources) to EAV
        /// </summary>
        /// <param name="partsDefinitions"></param>
        /// <param name="queryEntityGuid">EngityGuid of the Pipeline-Entity</param>
        private Dictionary<string, Guid> SavePartsAndGenerateRenameMap(List<Dictionary<string, object>> partsDefinitions,
            Guid queryEntityGuid)
        {
            Log.Add($"save parts guid:{queryEntityGuid}");
            var newDataSources = new Dictionary<string, Guid>();

            foreach (var ds in partsDefinitions)
            {
                // go case insensitive...
                var dataSource = new Dictionary<string, object>(ds, StringComparer.InvariantCultureIgnoreCase);
                // Skip Out-DataSource
                var originalIdentity = dataSource[Constants.EntityFieldGuid].ToString();
                dataSource.TryGetValue(Constants.EntityFieldId, out object entityId);

                // remove key-fields, as we cannot save them (would cause error)
                RemoveIdAndGuidFromValues(dataSource);

                if (originalIdentity == "Out") continue;

                // Update existing DataSource
                if (dataSource.ContainsKey(QueryConstants.VisualDesignerData))
                    dataSource[QueryConstants.VisualDesignerData] = dataSource[QueryConstants.VisualDesignerData].ToString(); // serialize this JSON into string

                if (entityId != null)
                    AppManager.Entities.UpdateParts(Convert.ToInt32(entityId), dataSource);
                // Add new DataSource
                else
                {
                    Tuple<int, Guid> entity = AppManager.Entities.Create(Constants.QueryPartTypeName, dataSource,
                        new Metadata.Target { TargetType = Constants.MetadataForEntity, KeyGuid = queryEntityGuid });
                    newDataSources.Add(originalIdentity, entity.Item2);
                }
            }

            return newDataSources;
        }


        /// <summary>
        /// micro helper - otherwise we run into errors when saving
        /// </summary>
        /// <param name="values"></param>
        private static void RemoveIdAndGuidFromValues(Dictionary<string, object> values)
        {
            values.Remove(Constants.EntityFieldGuid);
            values.Remove(Constants.EntityFieldId);
        }

        /// <summary>
        /// Delete Query Parts (DataSources) that are not present
        /// </summary>
        public void DeletedRemovedParts(
            List<Guid> newEntityGuids, 
            IEnumerable<Guid> newDataSources, 
            QueryDefinition qDef)
        {
            Log.Add($"delete part a#{AppManager.AppId}, pipe:{qDef.Entity.EntityGuid}");
            // Get EntityGuids currently stored in EAV
            var existingEntityGuids = qDef.Parts.Select(e => e.Guid);

            // Get EntityGuids from the UI (except Out and unsaved)
            newEntityGuids.AddRange(newDataSources);

            foreach (var entityToDelete in existingEntityGuids
                .Where(existingGuid => !newEntityGuids.Contains(existingGuid)))
                AppManager.Entities.Delete(entityToDelete);
        }



        /// <summary>
        /// Save a Query Entity to EAV
        /// </summary>
        /// <param name="id">EntityId of the Entity describing the Query</param>
        /// <param name="values"></param>
        /// <param name="wirings"></param>
        /// <param name="renamedDataSources">Array with new DataSources and the unsavedName and final EntityGuid</param>
        private void SaveHeader(int id, Dictionary<string, object> values, List<Connection> wirings, IDictionary<string, Guid> renamedDataSources)
        {
            Log.Add($"save pipe a#{AppManager.AppId}, pipe:{id}");
            wirings = RenameWiring(wirings, renamedDataSources);

            // Validate Stream Wirings, as we should never save bad wirings
            foreach (var wireInfo in wirings.Where(wireInfo => wirings.Count(w => w.To == wireInfo.To && w.In == wireInfo.In) > 1))
                throw new Exception(
                    $"DataSource \"{wireInfo.To}\" has multiple In-Streams with Name \"{wireInfo.In}\". Each In-Stream must have an unique Name and can have only one connection.");

            // add to new object...then send to save/update
            values[Constants.QueryStreamWiringAttributeName] = Connections.Serialize(wirings);
            AppManager.Entities.UpdateParts(id, values);
        }

        /// <summary>
        /// Update Wirings of Entities just added - as in the json-text they still have string-names, not the guids
        /// </summary>
        /// <param name="wirings"></param>
        /// <param name="renamedDataSources"></param>
        /// <returns></returns>
        private static List<Connection> RenameWiring(List<Connection> wirings, IDictionary<string, Guid> renamedDataSources)
        {
            if (renamedDataSources == null) return wirings;

            var wiringsNew = new List<Connection>();
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
