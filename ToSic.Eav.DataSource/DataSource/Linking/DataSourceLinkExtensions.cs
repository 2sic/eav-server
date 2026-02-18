namespace ToSic.Eav.DataSource;

internal static class DataSourceLinkExtensions
{
    public static int MaxRecursions = 10;

    /// <summary>
    /// Internal use only - flatten all the links in this object.
    /// </summary>
    /// <param name="link"></param>
    /// <param name="recursion">Upward counter, to exit if it exceeds a certain limit</param>
    /// <returns></returns>
    public static IEnumerable<IDataSourceLink> Flatten(this IDataSourceLink link, int recursion = 0)
    {
        var list = Enumerable.Empty<IDataSourceLink>();
        if (recursion > MaxRecursions)
            return [];
        list = list.Concat([link]);
        if (link.More.SafeAny())
            list = list.Concat(link.More.SelectMany(m => m.Flatten(recursion + 1)));
        return list;
    }
}