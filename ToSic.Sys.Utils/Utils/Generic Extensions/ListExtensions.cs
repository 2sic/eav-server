namespace ToSic.Sys.Utils;

public static class ListExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize) =>
        source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x
                .Select(v => v.Value)
                .ToList()
            )
            .ToList();
}