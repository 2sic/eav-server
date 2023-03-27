using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSource.Streams
{
    public static class DataStreamExtensions
    {
        public static bool HasStreamWithItems(this IDictionary<string, IDataStream> In, string streamName)
            => In.ContainsKey(streamName) && In[streamName]?.List.Any() == true;
    }
}
