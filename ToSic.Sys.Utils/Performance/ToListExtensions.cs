namespace ToSic.Sys.Performance;
public static class ToListExtensions
{
    public static IList<T> ToListOpt<T>(this IEnumerable<T> source)
        => SysPerfSettings.PreferArray
            ? source.ToArray()
            : source.ToList();

    public static IList<T> ToListOptSafe<T>(this IEnumerable<T>? source)
        => source == null
            ? new List<T>()
            : SysPerfSettings.PreferArray
                ? source.ToArray()
                : source.ToList();

}
