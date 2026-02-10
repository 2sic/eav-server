using System.Collections.ObjectModel;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSource.Sys.Streams;
using ToSic.Eav.DataSource.VisualQuery.Sys;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.Services;
using ToSic.Sys.Users;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// A DataSource that can access any other data source to retrieve information.
/// </summary>
/// <remarks>
/// This is meant for special scenarios where an exotic dropdown needs to get some data,
/// which would otherwise require a custom endpoint and a lot of overhead.
///
/// Using this data-source through the `System.SystemData` query,
/// you can specify the NameId of another data-source, and this will return the list of that data-source.
/// </remarks>
/// <inheritdoc />
[PrivateApi]
[VisualQuery(
    NiceName = "System Data",
    NameId = "37cf83f7-5e57-4c4a-9798-a7e1440f99b3",
    Type = DataSourceType.System,
    UiHint = "System-Data DataSource to access any other DataSource - use with caution.",
    Audience = Audience.System, // Never show in Visual Query, as it's too powerful
    DynamicOut = true
)]
// ReSharper disable once UnusedMember.Global
public sealed class SysData : CustomDataSource
{
    private readonly DataSourceCatalog _catalog;
    private readonly IDataSourcesService _dataSourceFactory;
    private readonly IUser _user;

    #region Configuration-properties

    /// <summary>
    /// The NameId of the DataSource to get data from.
    /// </summary>
    /// <remarks>
    /// It uses a fairly exotic name to avoid conflicts with parameter names of the data-sources being called.
    /// </remarks>
    [Configuration(Fallback = "", Token = $"[{ParamsSourceName}:{nameof(SysDataSource)}]")]
    public string SysDataSource => Configuration.GetThis(fallback: "");

    /// <summary>
    /// The NameId of the DataSource to get data from.
    /// </summary>
    /// <remarks>
    /// It uses a fairly exotic name to avoid conflicts with parameter names of the data-sources being called.
    /// </remarks>
    [Configuration(Fallback = StreamDefaultName, Token = $"[{ParamsSourceName}:{nameof(SysDataStream)}||{StreamDefaultName}]")]
    public string SysDataStream => Configuration.GetThis(fallback: StreamDefaultName);

    #endregion

    public SysData(Dependencies services, DataSourceCatalog catalog, IDataSourcesService dataSourceFactory, IUser user)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.SysData", connect: [catalog, dataSourceFactory])
    {
        _catalog = catalog;
        _dataSourceFactory = dataSourceFactory;
        _user = user;
        //ProvideOut(GetList);
    }

    public override IReadOnlyDictionary<string, IDataStream> Out => field ??= new ReadOnlyDictionary<string, IDataStream>(GenerateOut());

    public Dictionary<string, IDataStream> GenerateOut()
    {
        var streamNames = SysDataStream
            .UseFallbackIfNoValue(StreamDefaultName);

        var l = Log.Fn<Dictionary<string, IDataStream>>($"Streams: {streamNames}");
            
        var streams = streamNames
            .CsvToArrayWithoutEmpty()
            .Select(streamName => (Out: streamName, In: streamName))
            .ToList();

        if (!streams.Any())
        {
            streams = [(StreamDefaultName, StreamDefaultName)];
            l.A($"Stream list was empty, added default stream. Length: {streams.Count}");
        }
        else if (!streams.Any(pair => pair.Out.EqualsInsensitive(StreamDefaultName)))
        {
            var first = streams.First().In;
            streams.Insert(0, (StreamDefaultName, first));
            l.A($"Stream list was missing default, added it to mirror the first stream name '{first}'. Length: {streams.Count}");
        }

        // First get all the stream functions on the distinct In-stream names
        // So that we don't duplicate
        //var inStreams = streams
        //    .Select(pair => pair.In)
        //    .Distinct(StringComparer.InvariantCultureIgnoreCase)
        //    .ToDictionary(
        //        name => name,
        //        Func<IEnumerable<IEntity>> (name) => () => GetList(name),
        //        StringComparer.InvariantCultureIgnoreCase
        //    );

        var result = streams.ToDictionary(
            pair => pair.Out,
            IDataStream (pair) => new DataStream(Services.CacheService, this, pair.Out, () => GetOrUseCache(pair.In)),
            StringComparer.InvariantCultureIgnoreCase
        );

        return l.Return(result, $"Dic with {result.Count} entries");

        //var outDic = new Dictionary<string, IDataStream>(StringComparer.InvariantCultureIgnoreCase)
        //    {
        //        [DataSourceConstants.StreamDefaultName] = new DataStream(Services.CacheService, this, DataSourceConstants.StreamDefaultName, GetOrUseCache())
        //    };
        //return outDic;
    }

    private readonly Dictionary<string, IEnumerable<IEntity>> _cache = new(StringComparer.InvariantCultureIgnoreCase);

    private IEnumerable<IEntity> GetList()
    {
        return GetList(inStreamName: SysDataStream);
    }

    private IEnumerable<IEntity> GetOrUseCache(string inStreamName)
    {
        if (_cache.TryGetValue(inStreamName, out var cached))
            return cached;

        var result = GetList(inStreamName);
        // ReSharper disable PossibleMultipleEnumeration
        _cache[inStreamName] = result;
        return result;
        // ReSharper restore PossibleMultipleEnumeration
    }

    private IEnumerable<IEntity> GetList(string inStreamName)
    {
        // If nothing relevant specified, return trivial message
        if (string.IsNullOrWhiteSpace(SysDataSource))
            return GetTrivialMessage(false, inStreamName, false);

        // Try to find in catalog, if not found, return trivial message
        var dsInfo = _catalog.FindDataSourceInfo(SysDataSource, AppId);
        if (dsInfo == null)
            return GetTrivialMessage(false, inStreamName, false);

        if (!dsInfo.IsAllowed(_user))
            return GetTrivialMessage(true, SysDataStream, false, $"Not allowed, DataConfidentiality: {dsInfo.VisualQuery?.DataConfidentiality}");

        // Construct basic options and build the source
        var lookUp = CreateLookUpEngine();

        var options = new DataSourceOptions
        {
            AppIdentityOrReader = this.PureIdentity(),
            LookUp = lookUp,
        };
        var ds = _dataSourceFactory.Create(dsInfo.Type, options: options);

        // Get the stream, if not found, return trivial message, otherwise return the list
        var stream = ds.GetStream(inStreamName, nullIfNotFound: true);
        return stream?.List ?? GetTrivialMessage(true, inStreamName, false);
    }

    private ILookUpEngine CreateLookUpEngine()
    {
        var lookUp = Configuration.LookUpEngine;
        var qsLookUp = TryFindQueryStringSource(lookUp);
        if (qsLookUp == null)
            return lookUp;

        var newLookup = new LookUpInLookUps(MyConfigurationSourceName, [qsLookUp]);
        lookUp = new LookUpEngine(lookUp, Log, overrides: [newLookup]);

        return lookUp;
    }

    /// <summary>
    /// Helper to find the underlying source of the QueryString parameters,
    /// as we will pass these directly to the constructed data-source.
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    private static ILookUp? TryFindQueryStringSource(ILookUpEngine parent)
    {
        while (true)
        {
            var qsLookUp = parent.Sources.FirstOrDefault(l => l.Name.EqualsInsensitive("QueryString"));
            if (qsLookUp != null)
                return qsLookUp;
            if (parent.Downstream == null)
                return null;
            parent = parent.Downstream;
        }
    }

    private IEnumerable<IEntity> GetTrivialMessage(bool dsFound, string streamName, bool streamFound, string? allowed = default)
    {
        var l = Log.Fn<IEnumerable<IEntity>>();

        var dataFactory = DataFactory.SpawnNew(options: new()
        {
            TitleField = "Name",
            TypeName = nameof(SysData),
        });

        List<IEntity> result =
        [
            dataFactory.Create(new RawEntity(new()
            {
                { "Name", "SystemData DataSource - Error or Source/stream not found." },
                { nameof(SysDataSource), $"'{SysDataSource}' ({(dsFound ? "" : "not ")}found)" },
                { nameof(streamName), $"'{streamName}' ({(streamFound ? "" : "not ")}found)" },
                { "Allowed", allowed ?? "unknown" }
            }))
        ];

        return l.Return(result, $"{result.Count}");
    }
}