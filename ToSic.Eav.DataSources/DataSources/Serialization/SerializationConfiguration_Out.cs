using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Streams;
using ToSic.Lib.Helpers;
using static System.StringComparer;

namespace ToSic.Eav.DataSources
{
    public partial class Serialization
    {

        #region Dynamic Out

        /// <inheritdoc/>
        public override IDictionary<string, IDataStream> Out => _getOut.Get(CreateOutWithAllStreams);

        private readonly GetOnce<IDictionary<string, IDataStream>> _getOut = new GetOnce<IDictionary<string, IDataStream>>();

        /// <summary>
        /// Attach all missing streams, now that Out is used the first time.
        /// </summary>
        private IDictionary<string, IDataStream> CreateOutWithAllStreams()
        {
            var outDic = new Dictionary<string, IDataStream>(InvariantCultureIgnoreCase);
            foreach (var dataStream in In.Where(s => !outDic.ContainsKey(s.Key)))
                outDic.Add(dataStream.Key, new DataStream(this, dataStream.Key, () => GetList(dataStream.Key)));
            return outDic;
        }

        #endregion
    }
}
