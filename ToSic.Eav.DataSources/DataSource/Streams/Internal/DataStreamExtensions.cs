namespace ToSic.Eav.DataSource.Streams.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal static class DataStreamExtensions
{
    public static bool HasStreamWithItems(this IReadOnlyDictionary<string, IDataStream> inDic, string streamName)
        => inDic.ContainsKey(streamName) && inDic[streamName]?.List.Any() == true;
}