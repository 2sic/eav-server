using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSource;

namespace ToSic.Eav.DataSources
{
    public static class DataStreamExtensions
    {
        //public static bool HasRealStream(this IDictionary<string, IDataStream> In, string streamName) 
        //    => In.ContainsKey(streamName) && In[streamName] != null;

        public static bool HasStreamWithItems(this IDictionary<string, IDataStream> In, string streamName)
            => In.ContainsKey(streamName) && In[streamName]?.List.Any() == true;
    }
}
