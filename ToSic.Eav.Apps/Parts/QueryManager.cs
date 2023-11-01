using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.Query;
using ToSic.Lib.DI;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using static System.StringComparer;
using Connection = ToSic.Eav.DataSource.Query.Connection;
using Connections = ToSic.Eav.DataSource.Query.Connections;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// query manager to work with queries
    /// </summary>
    public partial class QueryManager: PartOf<AppManager>
    {
        private readonly LazySvc<AppWorkService> _appWorkServiceLazy;

        public QueryManager(
            LazySvc<SystemManager> systemManagerLazy,
            LazySvc<DataBuilder> builder,
            LazySvc<JsonSerializer> jsonSerializer,
            LazySvc<AppWorkService> appWorkServiceLazy,
            LazySvc<DataSource.Query.QueryManager> queryManager,
            LazySvc<QueryDefinitionBuilder> queryDefBuilder) : base("App.QryMng")
        {
            _appWorkServiceLazy = appWorkServiceLazy;
            ConnectServices(
                _systemManagerLazy = systemManagerLazy,
                _builder = builder,
                Serializer = jsonSerializer.SetInit(j => j.SetApp(Parent.AppState)),
                _queryManager = queryManager,
                _queryDefBuilder = queryDefBuilder
            );
        }

        private readonly LazySvc<SystemManager> _systemManagerLazy;
        private LazySvc<JsonSerializer> Serializer;
        private readonly LazySvc<DataSource.Query.QueryManager> _queryManager;
        private readonly LazySvc<QueryDefinitionBuilder> _queryDefBuilder;
        private readonly LazySvc<DataBuilder> _builder;

        private AppWorkService AppWorkSvc => _appWorkSvc ?? (_appWorkSvc = _appWorkServiceLazy.Value.Init(Parent.AppState));
        private AppWorkService _appWorkSvc;

        public bool Delete(int id)
        {
            var l = Log.Fn<bool>($"delete a#{Parent.AppId}, id:{id}");
            // Commented in v13, new implementation is based on AppState.Relationships
            var canDeleteResult = AppWorkSvc.EntityDelete.CanDeleteEntityBasedOnAppStateRelationshipsOrMetadata(id);
            if (!canDeleteResult.HasMessages)
                throw l.Done(new Exception(canDeleteResult.Messages));


            // Get the Entity describing the Query and Query Parts (DataSources)
            var queryEntity = _queryManager.Value.GetQueryEntity(id, AppWorkSvc.AppState);
            var qDef = _queryDefBuilder.Value.Create(queryEntity, AppWorkSvc.AppId);

            var parts = qDef.Parts;
            var mdItems = parts
                .Select(ds => ds.Entity.Metadata.FirstOrDefault())
                .Where(md => md != null)
                .Select(md => md.EntityId)
                .ToList();

            // delete in the right order - first the outermost-dependents, then a layer in, and finally the top node
            AppWorkSvc.EntityDelete.Delete(mdItems);
            AppWorkSvc.EntityDelete.Delete(parts.Select(p => p.Id).ToList());
            AppWorkSvc.EntityDelete.Delete(id);

            // flush cache
            _systemManagerLazy.Value.PurgeApp(AppWorkSvc.AppId);

            return l.ReturnTrue();
        }

        private QueryDefinition Get(int queryId)
            => _queryManager.Value.Get(AppWorkSvc.AppState, queryId);

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
            var qdef = Get(queryId);

            const string AllowEdit = "AllowEdit";
            if (qdef.Entity.GetBestValue<bool>(AllowEdit, Array.Empty<string>()) == false)
                throw new InvalidOperationException($"Query has {AllowEdit} set to false");

            var addedSources = SavePartsAndGenerateRenameMap(partDefs, qdef.Entity.EntityGuid);

            DeletedRemovedParts(newDsGuids, addedSources.Values, qdef);

            headerValues = new Dictionary<string, object>(headerValues, InvariantCultureIgnoreCase);
            RemoveIdAndGuidFromValues(headerValues);
            SaveHeader(queryId, headerValues, wirings, addedSources);
        }

        /// <summary>
        /// Save QueryParts (DataSources) to EAV
        /// </summary>
        /// <param name="partsDefinitions"></param>
        /// <param name="queryEntityGuid">EntityGuid of the Pipeline-Entity</param>
        private Dictionary<string, Guid> SavePartsAndGenerateRenameMap(List<Dictionary<string, object>> partsDefinitions, Guid queryEntityGuid)
        {
            Log.A($"save parts guid:{queryEntityGuid}");
            var newDataSources = new Dictionary<string, Guid>();

            foreach (var ds in partsDefinitions)
            {
                // go case insensitive...
                var dataSource = new Dictionary<string, object>(ds, InvariantCultureIgnoreCase);
                // Skip Out-DataSource
                var originalIdentity = dataSource[Attributes.EntityFieldGuid].ToString();
                dataSource.TryGetValue(Attributes.EntityFieldId, out var entityId);

                // remove key-fields, as we cannot save them (would cause error)
                RemoveIdAndGuidFromValues(dataSource);

                if (originalIdentity == "Out") continue;

                // Update existing DataSource
                if (dataSource.ContainsKey(QueryConstants.VisualDesignerData))
                    dataSource[QueryConstants.VisualDesignerData] = dataSource[QueryConstants.VisualDesignerData].ToString(); // serialize this JSON into string

                if (entityId != null)
                    AppWorkSvc.EntityUpdate.UpdateParts(Convert.ToInt32(entityId), dataSource);
                // Add new DataSource
                else
                {
                    var newSpecs = AppWorkSvc.EntityCreate.Create(QueryConstants.QueryPartTypeName, dataSource,
                        new Target((int)TargetTypes.Entity, null, keyGuid: queryEntityGuid));
                    newDataSources.Add(originalIdentity, newSpecs.EntityGuid);
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
            values.Remove(Attributes.EntityFieldGuid);
            values.Remove(Attributes.EntityFieldId);
        }

        /// <summary>
        /// Delete Query Parts (DataSources) that are not present
        /// </summary>
        public void DeletedRemovedParts(List<Guid> newEntityGuids, IEnumerable<Guid> newDataSources, QueryDefinition qDef)
        {
            var l = Log.Fn($"delete part a#{Parent.AppId}, pipe:{qDef.Entity.EntityGuid}");
            // Get EntityGuids currently stored in EAV
            var existingEntityGuids = qDef.Parts.Select(e => e.Guid);

            // Get EntityGuids from the UI (except Out and unsaved)
            newEntityGuids.AddRange(newDataSources);

            foreach (var entToDel in existingEntityGuids.Where(guid => !newEntityGuids.Contains(guid)))
                // force: true - force-delete the data-source part even if it still has metadata and stuff referencing it
                AppWorkSvc.EntityDelete.Delete(entToDel, force: true);
            l.Done();
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
            var l = Log.Fn($"save pipe a#{Parent.AppId}, pipe:{id}");
            wirings = RenameWiring(wirings, renamedDataSources, Log);

            // Validate Stream Wirings, as we should never save bad wirings
            foreach (var wireInfo in wirings.Where(wireInfo => wirings.Count(w => w.To == wireInfo.To && w.In == wireInfo.In) > 1))
                throw new Exception(
                    $"DataSource \"{wireInfo.To}\" has multiple In-Streams with Name \"{wireInfo.In}\". Each In-Stream must have an unique Name and can have only one connection.");

            // add to new object...then send to save/update
            values[QueryConstants.QueryStreamWiringAttributeName] = Connections.Serialize(wirings);
            // #ExtractEntitySave
            AppWorkSvc.EntityUpdate.UpdateParts(id, values);
            l.Done();
        }

        /// <summary>
        /// Update Wirings of Entities just added - as in the json-text they still have string-names, not the guids
        /// </summary>
        /// <param name="wirings"></param>
        /// <param name="renamedDataSources"></param>
        /// <param name="lg"></param>
        /// <returns></returns>
        private static List<Connection> RenameWiring(List<Connection> wirings, IDictionary<string, Guid> renamedDataSources, ILog lg)
        {
            var l = lg.Fn<List<Connection>>();
            if (renamedDataSources == null) 
                return l.Return(wirings, "no renames, no changes");

            var wiringsNew = new List<Connection>();
            foreach (var wireInfo in wirings)
            {
                var newWireInfo = wireInfo;
                if (renamedDataSources.TryGetValue(wireInfo.From, out var wFrom))
                    newWireInfo.From = wFrom.ToString();
                if (renamedDataSources.TryGetValue(wireInfo.To, out var wTo))
                    newWireInfo.To = wTo.ToString();
                wiringsNew.Add(newWireInfo);
            }
            return l.Return(wiringsNew, $"changed {wirings.Count}");
        }
    }
}
