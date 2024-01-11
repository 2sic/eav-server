using System.Collections.ObjectModel;
using ToSic.Eav.DataSource.Streams;
using ToSic.Lib.Helpers;
using static System.StringComparer;

namespace ToSic.Eav.DataSources;

partial class Serialization
{

    #region Dynamic Out

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, IDataStream> Out => _getOut.Get(() => new ReadOnlyDictionary<string, IDataStream>(CreateOutWithAllStreams()));

    private readonly GetOnce<IReadOnlyDictionary<string, IDataStream>> _getOut = new();

    /// <summary>
    /// Attach all missing streams, now that Out is used the first time.
    /// </summary>
    private IDictionary<string, IDataStream> CreateOutWithAllStreams()
    {
        var outDic = new Dictionary<string, IDataStream>(InvariantCultureIgnoreCase);
        foreach (var dataStream in In.Where(s => !outDic.ContainsKey(s.Key)))
            outDic.Add(dataStream.Key, new DataStream(Services.CacheService, this, dataStream.Key, () => GetList(dataStream.Key)));
        return outDic;
    }

    #endregion
}