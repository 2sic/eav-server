using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.EntityPair;

namespace ToSic.Eav.Data.Build;

internal class RawRelationshipsConvertHelper(AttributeAssembler attributeAssembler, ILog parentLog) : HelperBase(parentLog, "Eav.RawRel")
{
    /// <summary>
    /// Logging helper, to only log a few processes and then stop.
    /// </summary>
    [field: AllowNull, MaybeNull]
    private LogFilter RelationshipsToAttributesLogFilter => field
        ??= new(Log, logFirstMax: 25, reLogIteration: 100);

    /// <summary>
    /// Update a list of attributes to replace any RawRelationship values with a proper lookup attribute.
    /// </summary>
    /// <remarks>
    /// Note that the attribute itself does not resolve the relationships at this moment.
    /// It just creates an attribute which will do the lookup when ever needed.
    /// </remarks>
    /// <param name="values"></param>
    /// <param name="relationships">List of all entities which can be referenced</param>
    /// <returns></returns>
    internal Dictionary<string, object?> RelationshipsToAttributes(IDictionary<string, object?> values, ILookup<object, IEntity> relationships)
    {
        var l = RelationshipsToAttributesLogFilter.FnOrNull<Dictionary<string, object?>>();
        
        // Map all values to a new dictionary, preserving non-relationships and converting relationships
        var valuesWithRelationships = values
            .ToDictionary(
                v => v.Key,
                v =>
                {
                    // If the value is not a RawRelationship, return it as-is
                    if (v.Value is not RawRelationship rawRelationship)
                        return v.Value;

                    // Create lookup using the relationships
                    // which will later use the keys to query the relationships-lookup (which currently may be incomplete)
                    var lookupSource = new LookUpEntitiesSource<object>(rawRelationship.Keys, relationships);

                    // Create attribute using this new lookup source
                    var relAttr = attributeAssembler.Relationship(v.Key, lookupSource);
                    return relAttr;
                },
                StringComparer.InvariantCultureIgnoreCase
            );
        
        return l.Return(valuesWithRelationships, $"{valuesWithRelationships.Count}");
    }

    /// <summary>
    /// Finalize a lookup-list of relationships, adding all final entities.
    /// </summary>
    /// <remarks>
    /// This step is **non-functional** as it updates the lazy relationships list.
    /// </remarks>
    /// <param name="list"></param>
    /// <param name="lazyRelationships"></param>
    internal void AddRelationshipsToLookup(IList<EntityPair<IRawEntity>> list, LazyLookup<object, IEntity> lazyRelationships)
    {
        var l = Log.Fn();
        
        var itemsWithKeys = list
            .Select(pair =>
            {
                // Check if it has relationship keys
                var relKeys = (pair.Partner as IRelationshipKeys)?.RelationshipKeys?.ToListOpt();

                // If it has relationship keys, return a new EntityPair with the entity and the relationship keys
                return relKeys.SafeAny()
                    ? new EntityPair<IList<object>>(pair.Entity, relKeys)
                    : null;
            })
            .OfType<EntityPair<IList<object>>>()
            .ToListOpt();

        var keyMap = itemsWithKeys
            .SelectMany(
                // Create a KeyValuePair for each relationship key, mapping it to the entity
                pair => pair.Partner.Select(key => new KeyValuePair<object, IEntity>(key, pair.Entity))
            )
            .ToListOpt();

        if (keyMap.Any())
            lazyRelationships.Add(keyMap);

        l.Done($"Added {keyMap.Count}");
    }

}