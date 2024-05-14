using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build;

internal class RawRelationshipsConverter(DataBuilder builder, ILog parentLog) : HelperBase(parentLog, "Eav.RawRel")
{
    internal Dictionary<string, object> RelationshipsToAttributes(IDictionary<string, object> values, ILookup<object, IEntity> relationships)
    {
        var l = Log.Fn<Dictionary<string, object>>();
        var valuesWithRelationships = values.ToDictionary(
            v => v.Key,
            v =>
            {
                if (v.Value is not RawRelationship rawRelationship) return v.Value;
                var lookupSource =
                    new LookUpEntitiesSource<object>(rawRelationship.Keys, relationships);
                var relAttr = builder.Attribute.Relationship(v.Key, lookupSource);
                return relAttr;
            }, StringComparer.InvariantCultureIgnoreCase);
        return l.Return(valuesWithRelationships, $"{valuesWithRelationships.Count}");
    }

    internal void AddRelationshipsToLookup(List<ICanBeEntity> list, LazyLookup<object, IEntity> lazyRelationships, RawConvertOptions options)
    {
        var l = Log.Fn();
        var itemsWithKeys = list
            .Where(item => item is IEntityPair<IRawEntity>)
            .Cast<IEntityPair<IRawEntity>>()
            .Select(pair =>
            {
                var partner = pair.Partner as IHasRelationshipKeys;
                var relKeys = partner?.RelationshipKeys(options)?.ToList();
                return relKeys.SafeAny() ? new EntityPair<List<object>>(pair.Entity, relKeys) : null;
            })
            .Where(x => x!=null)
            .ToList();
        var keyMap = itemsWithKeys
            .SelectMany(pair => pair.Partner.Select(rk => new KeyValuePair<object, IEntity>(rk, pair.Entity)))
            .ToList();
        if (keyMap.Any()) lazyRelationships.Add(keyMap);

        l.Done($"Added {keyMap.Count}");
    }

}