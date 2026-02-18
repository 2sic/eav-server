namespace ToSic.Eav.DataSource.Sys.Convert;

/// <summary>
/// Helpers used by OData and ConvertToEavLight
/// </summary>
public class DataSourceConvertHelper
{
    /// <summary>
    /// Determine which stream(s) to use when a stream is specified, or not specified, and when all streams are requested.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string[] GetBestStreamNames(IDataSource query, string? stream)
    {
        // If somehow all streams are required, make sure it's null
        // so below it will default to using the query Out streams.
        if (stream == DataSourceConstants.AllStreams || stream.IsEmpty())
            stream = null;

        var streams = stream?.Split(',')
                          .Select(s => s.Trim())
                          .Where(s => !string.IsNullOrWhiteSpace(s))
                          .ToArray()
                      ?? query.Out
                          .Select(p => p.Key)
                          .ToArray();
        return streams;
    }

    public static HashSet<Guid> SafeParseGuidList(string[]? filterGuids)
    {
        var guidFilter = filterGuids?
                             .Select(g => Guid.TryParse(g, out var guid) ? guid : (Guid?)null)
                             .Where(g => g.HasValue)
                             .Select(g => g!.Value)
                             .ToHashSet()
                         ?? [];
        return guidFilter;
    }
}
