using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.Services;

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
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    ConfigurationType = "",
    NameId = "37cf83f7-5e57-4c4a-9798-a7e1440f99b3",
    NiceName = "System Data",
    Type = DataSourceType.System,
    UiHint = "System-Data DataSource to access any other DataSource - use with caution.",
    Audience = Audience.System, // Never show in Visual Query, as it's too powerful
    DynamicOut = false
)]
// ReSharper disable once UnusedMember.Global
public sealed class SystemData : CustomDataSource
{
    private readonly DataSourceCatalog _catalog;
    private readonly IDataSourcesService _dataSourceFactory;

    #region Configuration-properties

    /// <summary>
    /// The NameId of the DataSource to get data from.
    /// </summary>
    /// <remarks>
    /// It uses a fairly exotic name to avoid conflicts with parameter names of the data-sources being called.
    /// </remarks>
    [Configuration(Fallback = "", Token = $"[{DataSourceConstants.ParamsSourceName}:{nameof(SysDataSourceGuid)}]")]
    public string SysDataSourceGuid => Configuration.GetThis(fallback: "");

    /// <summary>
    /// The NameId of the DataSource to get data from.
    /// </summary>
    /// <remarks>
    /// It uses a fairly exotic name to avoid conflicts with parameter names of the data-sources being called.
    /// </remarks>
    [Configuration(Fallback = DataSourceConstants.StreamDefaultName, Token = $"[{DataSourceConstants.ParamsSourceName}:{nameof(SysDataStream)}]")]
    public string SysDataStream => Configuration.GetThis(fallback: DataSourceConstants.StreamDefaultName);

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// TODO
    /// </summary>
    [PrivateApi]
    public SystemData(Dependencies services, DataSourceCatalog catalog, IDataSourcesService dataSourceFactory) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.SysData", connect: [])
    {
        _catalog = catalog;
        _dataSourceFactory = dataSourceFactory;
        ProvideOut(GetList);
    }

    private IEnumerable<IEntity> GetList()
    {
        // If nothing relevant specified, return trivial message
        if (string.IsNullOrWhiteSpace(SysDataSourceGuid))
            return GetTrivialMessage(false, false);

        // Try to find in catalog, if not found, return trivial message
        var dsType = _catalog.FindDataSourceInfo(SysDataSourceGuid, AppId);
        if (dsType == null)
            return GetTrivialMessage(false, false);

        // Construct basic options and build the source
        var lookUp = CreateLookUpEngine();

        var options = new DataSourceOptions
        {
            AppIdentityOrReader = this.PureIdentity(),
            LookUp = lookUp,
        };
        var ds = _dataSourceFactory.Create(dsType.Type, options: options);

        // Get the stream, if not found, return trivial message, otherwise return the list
        var stream = ds.GetStream(SysDataStream, nullIfNotFound: true);
        return stream?.List ?? GetTrivialMessage(true, false);
    }

    private ILookUpEngine CreateLookUpEngine()
    {
        var lookUp = Configuration.LookUpEngine;
        var qsLookUp = TryFindQueryStringSource(lookUp);
        if (qsLookUp == null)
            return lookUp;

        var newLookup = new LookUpInLookUps(DataSourceConstants.MyConfigurationSourceName, [qsLookUp]);
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

    private IEnumerable<IEntity> GetTrivialMessage(bool dsFound, bool streamFound)
    {
        var l = Log.Fn<IEnumerable<IEntity>>();

        var dataFactory = DataFactory.SpawnNew(options: new()
        {
            TitleField = "Name",
            TypeName = nameof(SystemData),
        });

        List<IEntity> result =
        [
            dataFactory.Create(new RawEntity(new()
            {
                { "Name", "SystemData DataSource - Source with specified name not found." },
                { nameof(SysDataSourceGuid), $"{SysDataSourceGuid} ({(dsFound ? "" : "not ")}found)" },
                { nameof(SysDataStream), $"{SysDataStream} ({(streamFound ? "" : "not ")}found)" },
            }))
        ];

        return l.Return(result, $"{result.Count}");
    }
}