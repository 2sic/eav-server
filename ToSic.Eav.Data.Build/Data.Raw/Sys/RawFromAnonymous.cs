using System.Collections;

namespace ToSic.Eav.Data.Raw;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class RawFromAnonymous: RawEntity
{
    public RawFromAnonymous(object original, ILog log)
    {
        var dic = original.ToDicInvariantInsensitive(mutable: true);
            
        if (dic.ContainsKey(nameof(Id)))
        {
            Id = dic[nameof(Id)].ConvertOrDefault<int>();
            dic.Remove(nameof(Id));
        }

        if (dic.ContainsKey(nameof(Guid)))
        {
            Guid = dic[nameof(Guid)].ConvertOrDefault<Guid>();
            dic.Remove(nameof(Guid));
        }

        if (dic.ContainsKey(nameof(Created)))
        {
            Created = dic[nameof(Created)].ConvertOrDefault<DateTime>();
            dic.Remove(nameof(Created));
        }

        if (dic.ContainsKey(nameof(Modified)))
        {
            Modified = dic[nameof(Modified)].ConvertOrDefault<DateTime>();
            dic.Remove(nameof(Modified));
        }

        if (dic.ContainsKey(nameof(RelationshipKeys)))
        {
            var maybeRels = dic[nameof(RelationshipKeys)];
            if (maybeRels is IEnumerable rels && rels is not string)
                try
                {
                    _relationshipKeys = rels.Cast<object>().ToList();
                    // only remove if everything worked - so it stays in if something is wrong
                    // this will make it easier to spot issues
                    dic.Remove(nameof(RelationshipKeys));
                }
                catch
                {
                    log.E($"Error in {nameof(RawFromAnonymous)} trying to convert {nameof(RelationshipKeys)}");
                }
        }
        else
            _relationshipKeys = [];

        _relationshipKeys.Add(Id);

        // Scan relationships in dic...
        foreach (var key in dic.Keys.ToList() /* must copy the keys as we plan to change the dic */)
        {
            var val = dic[key];
            if (val == null || !val.IsAnonymous())
                continue;

            var maybeRefs = val.ObjectToDictionary(caseInsensitive: false);
            if (!maybeRefs.TryGetValue("Relationships", out var relsTemp) || relsTemp == null)
                continue;
                
            // If we only have on object / string, use it as key
            // if we have an ienumerable use the list as the keys
            var keys = relsTemp is IEnumerable relsList && relsTemp is not string
                ? relsList.Cast<object>().ToList()
                : [relsTemp];
            dic[key] = new RawRelationship(keys: keys);
        }

        Values = dic;
    }


    public override IEnumerable<object> RelationshipKeys(RawConvertOptions options) => _relationshipKeys;
    private readonly List<object> _relationshipKeys = [];
}