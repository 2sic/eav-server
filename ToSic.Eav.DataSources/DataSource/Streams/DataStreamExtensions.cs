using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSource.Streams;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class DataStreamExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool HasStreamWithItems(this IReadOnlyDictionary<string, IDataStream> In, string streamName)
        => In.ContainsKey(streamName) && In[streamName]?.List.Any() == true;
}