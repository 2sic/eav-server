using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.DataSources
{
    public static class DataStreamExtensions
    {
        public static bool HasRealStream(this IDictionary<string, IDataStream> In, string streamName) 
            => In.ContainsKey(streamName) && In[Constants.FallbackStreamName] != null;

        public static bool HasStreamWithItems(this IDictionary<string, IDataStream> In, string streamName)
            => In.HasRealStream(streamName) && In[streamName].Immutable.Any();
    }
}
