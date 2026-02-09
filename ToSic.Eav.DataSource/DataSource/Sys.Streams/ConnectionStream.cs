using System.Collections;
using ToSic.Eav.DataSource.Sys.Caching;

namespace ToSic.Eav.DataSource.Sys.Streams;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class ConnectionStream(
    LazySvc<IDataSourceCacheService> cache,
    DataSourceConnection connection,
    DataSourceErrorHelper errorHandler)
    : IDataStream, IWrapper<IDataStream>
{
    public DataSourceConnection Connection = connection;

    private IDataStream LoadStream()
    {
        if (Connection == null! /* paranoid check, but really impossible */)
            return CreateErrorStream("Missing Connection", "ConnectionStream can't LoadStream()");

        var dataSource = Connection.Source;
        var streamName = Connection.SourceStream;
        if (dataSource == null! || string.IsNullOrEmpty(streamName))
            return Connection.DirectlyAttachedStream 
                   ?? CreateErrorStream("Missing Source or Name",
                       $"LoadStream(): No Stream and name or source were also missing - name: '{streamName}', source: '{dataSource}'");

        if (!dataSource.Out.TryGetValue(streamName, out var stream))
            return CreateErrorStream("Source doesn't have Stream", $"LoadStream(): Source '{dataSource.Label}' doesn't have stream '{streamName}'", dataSource);
        
        return stream
            ?? CreateErrorStream("Source Stream is Null", $"Source '{dataSource.Label}' has stream '{streamName}' but it's null", dataSource);

    }

    private IDataStream CreateErrorStream(string title, string message, IDataSource? intendedSource = null)
    {
        var errors = errorHandler.Create(title: title, message: message);
        return new DataStream(
            cache,
            intendedSource!,    // this is an edge case, where there is actually not real source, but we must still create an error-stream
            "ConnectionStreamError",
            () => errors
        );
    }

    public IDataStream GetContents() => InnerStream;
    private IDataStream InnerStream => _dataStream.Get(LoadStream)!;
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

    public IDataSourceLink GetLink() => InnerStream.GetLink();
}