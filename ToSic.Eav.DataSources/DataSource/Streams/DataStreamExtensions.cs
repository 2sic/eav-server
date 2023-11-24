using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSource.Streams;

public static class DataStreamExtensions
{
    public static bool HasStreamWithItems(this IReadOnlyDictionary<string, IDataStream> In, string streamName)
        => In.ContainsKey(streamName) && In[streamName]?.List.Any() == true;
}