using System.Collections;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Lib.Data;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSource.Streams.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal class ConnectionStream(
    LazySvc<IDataSourceCacheService> cache,
    DataSourceConnection connection,
    DataSourceErrorHelper errorHandler = null)
    : IDataStream, IWrapper<IDataStream>
{
    public DataSourceConnection Connection = connection;

    private IDataStream LoadStream()
    {
        if (Connection == null) 
            return CreateErrorStream("Missing Connection", "ConnectionStream can't LoadStream()");

        var ds = Connection.DataSource;
        var name = Connection.SourceStream;
        IDataStream stream;
        var noSource = ds == null;
        var noName = string.IsNullOrEmpty(name);
        if (noSource || noName)
        {
            stream = Connection.DirectlyAttachedStream;
            if (stream == null)
                return CreateErrorStream("Missing Source or Name", 
                    $"LoadStream(): No Stream and name or source were also missing - name: '{name}', source: '{ds}'");
        }
        else
        {
            if (!ds.Out.ContainsKey(name))
                return CreateErrorStream("Source doesn't have Stream", $"LoadStream(): Source '{ds.Label}' doesn't have stream '{name}'", ds);
            stream = ds.Out[name];
            if (stream == null)
                return CreateErrorStream("Source Stream is Null", $"Source '{ds.Label}' has stream '{name}' but it's null", ds);
        }

        return stream;
    }

    private IDataStream CreateErrorStream(string title, string message, IDataSource intendedSource = null)
    {
        var errors = errorHandler.Create(title: title, message: message);
        return new DataStream(cache, intendedSource, "ConnectionStreamError", () => errors);
    }

    public IDataStream GetContents() => InnerStream;
    private IDataStream InnerStream => _dataStream.Get(LoadStream);
    private readonly GetOnce<IDataStream> _dataStream = new();


    #region Simple properties linked to the underlying Stream

    public bool AutoCaching => InnerStream.AutoCaching;

    public int CacheDurationInSeconds
    {
        get => InnerStream.CacheDurationInSeconds;
        set => InnerStream.CacheDurationInSeconds = value;
    }

    public bool CacheRefreshOnSourceRefresh
    {
        get => InnerStream.CacheRefreshOnSourceRefresh;
        set => InnerStream.CacheRefreshOnSourceRefresh = value;
    }

    public IEnumerator<IEntity> GetEnumerator() => InnerStream.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => InnerStream.GetEnumerator();

    public IEnumerable<IEntity> List => InnerStream.List;

    public IDataSource Source => InnerStream.Source;

    public string Name => InnerStream.Name;
    public string Scope => InnerStream.Scope;

    public DataStreamCacheStatus Caching => InnerStream.Caching;
    public void ResetStream() => InnerStream.ResetStream();

    #endregion

    public IDataSourceLink Link => InnerStream.Link;
}