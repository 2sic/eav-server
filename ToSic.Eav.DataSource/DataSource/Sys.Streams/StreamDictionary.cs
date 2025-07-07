using System.Collections.ObjectModel;
using ToSic.Eav.DataSource.Sys.Caching;

namespace ToSic.Eav.DataSource.Sys.Streams;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class StreamDictionary
{
    private readonly LazySvc<IDataSourceCacheService> _dsCache;
    internal IDataSource? Source;

    private readonly Dictionary<string, IDataStream> _inner = new(StringComparer.InvariantCultureIgnoreCase);

    public StreamDictionary(LazySvc<IDataSourceCacheService> dsCache)
    {
        _dsCache = dsCache;
    }

    /// <summary>
    /// Re-bundle an existing set of streams for the new Source which will provide it
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dsCache"></param>
    /// <param name="streams"></param>
    public StreamDictionary(IDataSource source, LazySvc<IDataSourceCacheService> dsCache, IReadOnlyDictionary<string, IDataStream>? streams = default)
    {
        Source = source;
        _dsCache = dsCache;
        if (streams == null)
            return;

        foreach (var stream in streams)
            Add(stream.Key, WrapStream(Source, stream.Key, stream.Value));
    }

    public void Add(string name, IDataStream stream)
    {
        _inner[name] = Source == null
            ? stream
            : WrapStream(Source, name, stream);
        _ro.Reset();
    }

    public void Clear()
        => _inner.Clear();

    public bool ContainsKey(string name)
        => _inner.ContainsKey(name);

    private IDataStream WrapStream(IDataSource source, string name, IDataStream stream)
        => new DataStream(_dsCache, source, name, () => stream.List) { Scope = stream.Scope };

    public IReadOnlyDictionary<string, IDataStream> AsReadOnly() => _ro.Get(() => new ReadOnlyDictionary<string, IDataStream>(_inner))!;
    private readonly GetOnce<IReadOnlyDictionary<string, IDataStream>> _ro = new();
}