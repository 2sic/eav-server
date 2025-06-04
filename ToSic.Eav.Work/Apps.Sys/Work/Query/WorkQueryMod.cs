using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Eav.Metadata;
using static System.StringComparer;
using Connection = ToSic.Eav.DataSource.Internal.Query.Connection;
using Connections = ToSic.Eav.DataSource.Internal.Query.Connections;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkQueryMod(
    LazySvc<AppCachePurger> systemManagerLazy,
    LazySvc<QueryManager> queryManager,
    LazySvc<QueryDefinitionBuilder> queryDefBuilder,
    GenWorkDb<WorkEntityCreate> entCreate,
    GenWorkDb<WorkEntityDelete> delete,
    GenWorkDb<WorkEntityUpdate> entUpdate)
    : WorkUnitBase<IAppWorkCtx>("AWk.QryMod",
        connect: [systemManagerLazy, queryManager, queryDefBuilder, delete, entCreate, entUpdate])
{
    public bool Delete(int id)
    {
        var l = Log.Fn<bool>($"delete a#{AppWorkCtx.AppId}, id:{id}");

        var entDelete = delete.New(AppWorkCtx.AppReader);

        var canDeleteResult = entDelete.CanDeleteEntityBasedOnAppStateRelationshipsOrMetadata(id);
        if (!canDeleteResult.HasMessages)
            throw l.Done(new Exception(canDeleteResult.Messages));


        // Get the Entity describing the Query and Query Parts (DataSources)
        var queryEntity = queryManager.Value.GetQueryEntity(id, AppWorkCtx.AppReader);
        var qDef = queryDefBuilder.Value.Create(queryEntity, AppWorkCtx.AppId);

        var parts = qDef.Parts;
        var mdItems = parts
            .Select(ds => ds.Entity.Metadata.FirstOrDefault())
            .Where(md => md != null)
            .Select(md => md.EntityId)
            .ToList();

        // delete in the right order - first the outermost-dependents, then a layer in, and finally the top node
        entDelete.Delete(mdItems);
        entDelete.Delete(parts.Select(p => p.Id).ToList());
        entDelete.Delete(id);

        // flush cache
        systemManagerLazy.Value.PurgeApp(AppWorkCtx.AppId);

        return l.ReturnTrue();
    }

    private QueryDefinition Get(int queryId)
        => queryManager.Value.Get(AppWorkCtx.AppReader, queryId);

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

        const string allowEdit = "AllowEdit";
        if (!qdef.Entity.Get<bool>(allowEdit))
            throw new InvalidOperationException($"Query has {allowEdit} set to false");

        var addedSources = SavePartsAndGenerateRenameMap(partDefs, qdef.Entity.EntityGuid);

        DeletedRemovedParts(newDsGuids, addedSources.Values, qdef);

        headerValues = new(headerValues, InvariantCultureIgnoreCase);
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
            var originalIdentity = dataSource[AttributeNames.EntityFieldGuid].ToString();
            dataSource.TryGetValue(AttributeNames.EntityFieldId, out var entityId);

            // remove key-fields, as we cannot save them (would cause error)
            RemoveIdAndGuidFromValues(dataSource);

            if (originalIdentity == "Out") continue;

            // Update existing DataSource
            if (dataSource.ContainsKey(QueryConstants.VisualDesignerData))
                dataSource[QueryConstants.VisualDesignerData] = dataSource[QueryConstants.VisualDesignerData].ToString(); // serialize this JSON into string

            if (entityId != null)
                entUpdate.New(AppWorkCtx.AppReader).UpdateParts(Convert.ToInt32(entityId), dataSource, new());
            // Add new DataSource
            else
            {
                var newSpecs = entCreate.New(AppWorkCtx.AppReader).Create(QueryConstants.QueryPartTypeName, dataSource,
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
        values.Remove(AttributeNames.EntityFieldGuid);
        values.Remove(AttributeNames.EntityFieldId);
    }

    /// <summary>
    /// Delete Query Parts (DataSources) that are not present
    /// </summary>
    private void DeletedRemovedParts(List<Guid> newEntityGuids, IEnumerable<Guid> newDataSources, QueryDefinition qDef)
    {
        var l = Log.Fn($"delete part a#{AppWorkCtx.AppId}, pipe:{qDef.Entity.EntityGuid}");
        // Get EntityGuids currently stored in EAV
        var existingEntityGuids = qDef.Parts.Select(e => e.Guid);

        // Get EntityGuids from the UI (except Out and unsaved)
        newEntityGuids.AddRange(newDataSources);

        var entDelete = delete.New(AppWorkCtx.AppReader);
        foreach (var entToDel in existingEntityGuids.Where(guid => !newEntityGuids.Contains(guid)))
            // force: true - force-delete the data-source part even if it still has metadata and stuff referencing it
            entDelete.Delete(entToDel, force: true);
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
        var l = Log.Fn($"save pipe a#{AppWorkCtx.AppId}, pipe:{id}");
        wirings = RenameWiring(wirings, renamedDataSources, Log);

        // Validate Stream Wirings, as we should never save bad wirings
        foreach (var wireInfo in wirings.Where(wireInfo => wirings.Count(w => w.To == wireInfo.To && w.In == wireInfo.In) > 1))
            throw new(
                $"DataSource \"{wireInfo.To}\" has multiple In-Streams with Name \"{wireInfo.In}\". Each In-Stream must have an unique Name and can have only one connection.");

        // add to new object...then send to save/update
        values[QueryConstants.QueryStreamWiringAttributeName] = Connections.Serialize(wirings);
        entUpdate.New(AppWorkCtx.AppReader).UpdateParts(id, values, new());
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