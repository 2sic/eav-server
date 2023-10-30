using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.Generics;
using ToSic.Eav.Metadata;
using ToSic.Lib.Logging;
using Connection = ToSic.Eav.DataSource.Query.Connection;
using Connections = ToSic.Eav.DataSource.Query.Connections;

namespace ToSic.Eav.Apps.Parts
{
    public partial class QueryManager
    {

        public void SaveCopy(int id) => SaveCopy(Get(id));

        public void SaveCopy(QueryDefinition query) => Log.Do(() =>
        {
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
                _valueBuilder.Value.String(newWiring)
            };
            var queryAttributes = query.Entity.Attributes.ToEditable();
            var newWiringAttribute =
                _builder.Value.Attribute.CreateFrom(queryAttributes[QueryConstants.QueryStreamWiringAttributeName],
                    newWiringValues.ToImmutableList());
            queryAttributes[QueryConstants.QueryStreamWiringAttributeName] = newWiringAttribute;

            var newQuery = _builder.Value.Entity.CreateFrom(query.Entity, id: 0, guid: newQueryGuid, attributes: _builder.Value.Attribute.Create(queryAttributes));

            var saveList = newParts.Select(p => p.Value).Concat(newMetadata).ToList();
            saveList.Add(newQuery);
            Parent.Entities.Save(saveList);
        });



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
            var newSer = Serializer.Value.Serialize(original);
            var newEnt = Serializer.Value.Deserialize(newSer);

            newEnt = _builder.Value.Entity.CreateFrom(newEnt, guid: newGuid, id: 0,
                target: newMetadataTarget == null
                    ? null
                    : new Target(original.MetadataFor, keyGuid: newMetadataTarget.Value)
            );
            return l.Return(newEnt);
        }
    }
}
