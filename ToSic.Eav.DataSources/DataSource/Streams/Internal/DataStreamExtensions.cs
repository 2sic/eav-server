namespace ToSic.Eav.DataSource.Streams.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
internal static class DataStreamExtensions
{
    public static bool HasStreamWithItems(this IReadOnlyDictionary<string, IDataStream> inDic, string streamName)
        => inDic.ContainsKey(streamName) && inDic[streamName]?.List.Any() == true;
}