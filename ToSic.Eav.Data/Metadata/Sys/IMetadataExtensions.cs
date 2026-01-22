namespace ToSic.Eav.Metadata.Sys;

public static class IMetadataExtensions
{
    public static T? GetMetadata<T>(this IHasMetadata obj)
        where T : IWrapperSetup<IEntity>, new()
        => obj.Metadata.GetMetadata<T>(typeof(T).Name);

    public static T? GetMetadata<T>(this IHasMetadata obj, string nameId)
        where T : IWrapperSetup<IEntity>, new()
        => obj.Metadata.GetMetadata<T>(nameId);

    public static T? GetMetadata<T>(this IMetadata? md, string nameId)
        where T : IWrapperSetup<IEntity>, new()
    {
        if (md == null)
            return default;

        var list = md.OfType(nameId).ToList();
        if (list.Count == 0)
            return default;

        var first = list.FirstOrDefault();
        if (first == null)
            return default;

        var result = new T();
        result.SetupContents(first);

        return result;
    }

    public static IEnumerable<T> GetMetadataList<T>(this IHasMetadata obj)
        where T : IWrapperSetup<IEntity>, new()
        => obj.GetMetadataList<T>(typeof(T).Name);

    public static IEnumerable<T> GetMetadataList<T>(this IHasMetadata obj, string nameId)
        where T : IWrapperSetup<IEntity>, new()
        => obj.Metadata.GetMetadataList<T>(nameId);

    public static IEnumerable<T> GetMetadataList<T>(this IMetadata? md, string nameId)
        where T : IWrapperSetup<IEntity>, new()
    {
        if (md == null)
            return [];

        var list = md
            .OfType(nameId)
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
                one.SetupContents(raw);
                return one;
            })
            .ToList();

        return result;
    }
}
