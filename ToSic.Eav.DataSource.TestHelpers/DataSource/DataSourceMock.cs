using ToSic.Eav.Data;
using ToSic.Sys.Caching.Keys;
using ToSic.Sys.Logging;

namespace ToSic.Eav.DataSource;

public class DataSourceMock: IDataSource
{
    public IDataSourceLink GetLink() => _link ??= new DataSourceLink { DataSource = this };

    private IDataSourceLink? _link;

    public int ZoneId => 0;
    public int AppId => 0;

    public string CachePartialKey => "DummyCacheKeyPartial";

    public string CacheFullKey => $".DummyCacheKeyFull.{CachePartialKey}";

    public long CacheTimestamp => 0;

    public bool CacheChanged(long dependentTimeStamp) => false;

    public ILog? Log => field ??= new Log("DS.mock");

    public void Attach(IDataSource dataSource)
    {
        throw new NotImplementedException();
    }

    public void Attach(string streamName, IDataSource dataSource, string sourceName = DataSourceConstants.StreamDefaultName)
    {
        throw new NotImplementedException();
    }

    public void Attach(string streamName, IDataStream dataStream)
    {
        throw new NotImplementedException();
    }

    public Guid Guid => Guid.Empty;


    public const string DefaultName = "Mock DataSource";
    public string Name { get; set; } = DefaultName;

    public string Label => Name;

    public void AddDebugInfo(Guid? guid, string? label)
    {
        throw new NotImplementedException();
    }
    public IReadOnlyDictionary<string, IDataStream> Out => field ??= new Dictionary<string, IDataStream>();

    public IDataStream? this[string outName] => throw new NotImplementedException();

    public IDataStream? GetStream(string? name = null, NoParamOrder npo = default, bool nullIfNotFound = false,
        bool emptyIfNotFound = false)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEntity> List => [];

    public IReadOnlyDictionary<string, IDataStream> In => field ??= new Dictionary<string, IDataStream>();

    public IDataSourceConfiguration Configuration { get; }

    public void Setup(IDataSourceOptions? options, IDataSourceLinkable? attach)
    {
        throw new NotImplementedException();
    }

    public List<string> CacheRelevantConfigurations { get; }
    
    public ICacheKeyManager CacheKey { get; }
    
    public DataSourceErrorHelper Error { get; }
    
    public bool Immutable => true;

    public void DoWhileOverrideImmutable(Action action)
    {
        throw new NotImplementedException();
    }
}
