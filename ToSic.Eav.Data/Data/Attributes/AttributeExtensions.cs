using ToSic.Eav.Data.Sys.Attributes;

namespace ToSic.Eav.Data;

public static class AttributeExtensions
{
    public static bool IsEntity(this IAttribute attribute) => attribute.Type == ValueTypes.Entity;
    public static bool IsEntity(this KeyValuePair<string, IAttribute> attribute) => attribute.Value.Type == ValueTypes.Entity;

    public static IEnumerable<KeyValuePair<string, IAttribute<IEnumerable<IEntity>>>> GetEntityAttributes(this IEnumerable<KeyValuePair<string, IAttribute>> attributes, bool filterNull = true)
    {
        var children = attributes
            .Where(a => a.IsEntity())
            .Select(pair => new KeyValuePair<string, IAttribute<IEnumerable<IEntity>>>(
                pair.Key,
                (IAttribute<IEnumerable<IEntity>>)pair.Value
            ));
            //.Cast<KeyValuePair<string, IAttribute<IEnumerable<IEntity>>>>();

        // safety check, should not be null but just in case, because we saw code in the Dump which explicitly said it had to filter nulls
        // comment was: "apparently sometimes the entities inside seem to be non-existent on Resources"
        if (filterNull)
            children = children.Where(pair => pair.Value != null);

        foreach (var child in children)
            yield return child;
    }
}
