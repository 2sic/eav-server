using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.EntityPair;

namespace ToSic.Eav.Data.Build;

internal class RawRelationshipsConvertHelper(DataAssembler dataAssembler, ILog parentLog) : HelperBase(parentLog, "Eav.RawRel")
{
    [field: AllowNull, MaybeNull]
    private LogFilter RelationshipsToAttributesLogFilter => field
        ??= new(Log, logFirstMax: 25, reLogIteration: 100);

    internal Dictionary<string, object?> RelationshipsToAttributes(IDictionary<string, object?> values, ILookup<object, IEntity> relationships)
    {
        var l = RelationshipsToAttributesLogFilter.FnOrNull<Dictionary<string, object?>>();
        var valuesWithRelationships = values.ToDictionary(
            v => v.Key,
            v =>
            {
                if (v.Value is not RawRelationship rawRelationship) return v.Value;
                var lookupSource =
                    new LookUpEntitiesSource<object>(rawRelationship.Keys, relationships);
                var relAttr = dataAssembler.Attribute.Relationship(v.Key, lookupSource);
                return relAttr;
            }, StringComparer.InvariantCultureIgnoreCase);
        return l.Return(valuesWithRelationships, $"{valuesWithRelationships.Count}");
    }

    internal void AddRelationshipsToLookup(IList<ICanBeEntity> list, LazyLookup<object, IEntity> lazyRelationships, RawConvertOptions options)
    {
        var l = Log.Fn();
        var itemsWithKeys = list
            .Where(item => item is IEntityPair<IRawEntity>)
            .Cast<IEntityPair<IRawEntity>>()
            .Select(pair =>
            {
                var partner = pair.Partner as IHasRelationshipKeys;
                var relKeys = partner?.RelationshipKeys(options)?.ToListOpt();
                return relKeys.SafeAny()
                    ? new EntityPair<IList<object>>(pair.Entity, relKeys)
                    : null;
            })
            .Where(x => x != null)
            .ToListOpt();

        var keyMap = itemsWithKeys
            .SelectMany(pair => pair!.Partner.Select(rk => new KeyValuePair<object, IEntity>(rk, pair.Entity)))
            .ToListOpt();

        if (keyMap.Any())
            lazyRelationships.Add(keyMap);

        l.Done($"Added {keyMap.Count}");
    }

}