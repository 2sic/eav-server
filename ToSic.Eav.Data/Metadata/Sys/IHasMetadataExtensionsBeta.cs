using ToSic.Eav.Data.Wrap;

namespace ToSic.Eav.Metadata.Sys;

public static class IHasMetadataExtensionsBeta
{
    public static T GetMetadata<T>(this IHasMetadata obj)
        where T : ICanWrapEavBeta<IEntity>, new()

    {
        var md = obj.Metadata;
        if (md == null)
            return default;

        var list = md.OfType(typeof(T).Name).ToList();
        if (list.Count == 0)
            return default;

        var first = list.FirstOrDefault();
        if (first == null)
            return default;

        var result = new T();
        result.Setup(first);

        return result;
    }

    public static TList GetMetadata<TList, T>(this IHasMetadata obj)
        where TList : List<T>, new()
        where T : ICanWrapEavBeta<IEntity>, new()

    {
        var md = obj.Metadata;
        if (md == null)
            return [];

        var list = md
            .OfType(typeof(T).Name)
            .Where(x => x != null)
            .ToList();

        if (list.Count == 0)
            return [];

        var first = list.FirstOrDefault();
        if (first == null)
            return [];

        var result = list
            .Select(raw =>
            {
                var one = new T();
                one.Setup(raw);
                return one;
            })
            .ToList();

        return (TList)result;
    }

}
