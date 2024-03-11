using System.Collections.ObjectModel;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.Generics;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSource.Streams.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class StreamDictionary
{
    private readonly LazySvc<IDataSourceCacheService> _cache;
    internal IDataSource Source;

    private readonly DictionaryInvariant<IDataStream> _inner = [];

    public StreamDictionary(LazySvc<IDataSourceCacheService> cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Re-bundle an existing set of streams for the new Source which will provide it
    /// </summary>
    /// <param name="source"></param>
    /// <param name="streams"></param>
    public StreamDictionary(IDataSource source, IReadOnlyDictionary<string, IDataStream> streams = default)
    {
        Source = source;
        if (streams == null) return;

        foreach (var stream in streams)
            Add(stream.Key, WrapStream(stream.Key, stream.Value));
    }

    public void Add(string name, IDataStream stream)
    {
        _inner[name] = Source == null ? stream : WrapStream(name, stream);
        _ro.Reset();
    }

    public void Clear() => _inner.Clear();

    public bool ContainsKey(string name) => _inner.ContainsKey(name);

    private IDataStream WrapStream(string name, IDataStream stream) =>
        new DataStream(_cache, Source, name, () => stream.List) { Scope = stream.Scope };

    public IReadOnlyDictionary<string, IDataStream> AsReadOnly() => _ro.Get(() => new ReadOnlyDictionary<string, IDataStream>(_inner));
    protected GetOnce<IReadOnlyDictionary<string, IDataStream>> _ro = new();
}