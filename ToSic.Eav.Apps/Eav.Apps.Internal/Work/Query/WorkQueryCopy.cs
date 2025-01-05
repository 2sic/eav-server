using System.Collections.Immutable;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Eav.Generics;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization.Internal;
using Connection = ToSic.Eav.DataSource.Internal.Query.Connection;
using Connections = ToSic.Eav.DataSource.Internal.Query.Connections;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkQueryCopy: WorkUnitBase<IAppWorkCtx>
{

    public WorkQueryCopy(
        LazySvc<QueryManager> queryManager,
        LazySvc<DataBuilder> builder,
        LazySvc<JsonSerializer> jsonSerializer,
        GenWorkDb<WorkEntitySave> entSave) : base("AWk.QryMod", connect: [entSave, queryManager, jsonSerializer, builder])
    {
        _entSave = entSave;
        _queryManager = queryManager;
        _serializer = jsonSerializer.SetInit(j => j.SetApp(AppWorkCtx.AppReader));
        _builder = builder;
    }

    private readonly GenWorkDb<WorkEntitySave> _entSave;
    private readonly LazySvc<DataBuilder> _builder;
    private readonly LazySvc<QueryManager> _queryManager;
    private readonly LazySvc<JsonSerializer> _serializer;

    private QueryDefinition Get(int queryId) => _queryManager.Value.Get(AppWorkCtx.AppReader, queryId);


    public void SaveCopy(int id) => SaveCopy(Get(id));

    public void SaveCopy(QueryDefinition query)
    {
        var l = Log.Fn();
        // Guid of the new query, which we'll need early on as target for the part-copies
        var newQueryGuid = Guid.NewGuid();

        // Prepare new Parts - copy and set target to the newQueryGuid
        // The dictionary must remember what the original guid was for mapping later on
        var newParts = query.Parts.ToDictionary(o => o.Guid,
            o => CopyAndResetIds(o.Entity, Guid.NewGuid(), newMetadataTarget: newQueryGuid));

        // Get Parts metadata (which points to the old parts) and point it to the new parts
        var newMetadata = query.Parts
            .Select(o => new { o.Guid, Value = o.Entity.Metadata.FirstOrDefault() })
            .Where(m => m.Value != null)
            .Select(o => CopyAndResetIds(o.Value, Guid.NewGuid(), newMetadataTarget: newParts[o.Guid].EntityGuid));

        // Remap the wiring-list of the data-sources from old to new
        var keyMap = newParts.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.EntityGuid.ToString());
        var newWiring = RemapWiringToCopy(query.Connections, keyMap);


        var newWiringValues = new List<IValue>
        {
            _builder.Value.Value.String(newWiring)
        };
        var queryAttributes = query.Entity.Attributes.ToEditable();
        var newWiringAttribute =
            _builder.Value.Attribute.CreateFrom(queryAttributes[QueryConstants.QueryStreamWiringAttributeName],
                newWiringValues.ToImmutableList());
        queryAttributes[QueryConstants.QueryStreamWiringAttributeName] = newWiringAttribute;

        var newQuery = _builder.Value.Entity.CreateFrom(query.Entity, id: 0, guid: newQueryGuid, attributes: _builder.Value.Attribute.Create(queryAttributes));

        var saveList = newParts.Select(p => p.Value).Concat(newMetadata).ToList();
        saveList.Add(newQuery);

        _entSave.New(AppWorkCtx.AppReader).Save(saveList);
        l.Done();
    }



    private static string RemapWiringToCopy(IList<Connection> origWiring, Dictionary<string, string> keyMap)
    {
        var wiringsClone = new List<Connection>();
        if (origWiring == null) return Connections.Serialize(wiringsClone);

        foreach (var wireInfo in origWiring)
        {
            var wireInfoClone = wireInfo; // creates a clone of the Struct
            if (keyMap.TryGetValue(wireInfo.From, out var wFrom))
                wireInfoClone.From = wFrom;
            if (keyMap.TryGetValue(wireInfo.To, out var wTo))
                wireInfoClone.To = wTo;

            wiringsClone.Add(wireInfoClone);
        }
        var newWiring = Connections.Serialize(wiringsClone);
        return newWiring;
    }

    private IEntity CopyAndResetIds(IEntity original, Guid newGuid, Guid? newMetadataTarget = null)
    {
        var l = Log.Fn<IEntity>();
        // todo: probably replace with clone as that should be reliable now...
        var newSer = _serializer.Value.Serialize(original);
        var newEnt = _serializer.Value.Deserialize(newSer);

        newEnt = _builder.Value.Entity.CreateFrom(newEnt, guid: newGuid, id: 0,
            target: newMetadataTarget == null
                ? null
                : new Target(original.MetadataFor, keyGuid: newMetadataTarget.Value)
        );
        return l.Return(newEnt);
    }
}