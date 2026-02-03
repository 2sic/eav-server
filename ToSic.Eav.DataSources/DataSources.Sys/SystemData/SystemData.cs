using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.DataSource.Sys.Catalog;
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
    [Configuration(Fallback = "", Token = $"[{DataSourceConstants.ParamsSourceName}:{nameof(DataSourceGuid)}]")]
    public string DataSourceGuid => Configuration.GetThis(fallback: "");

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
        if (string.IsNullOrWhiteSpace(DataSourceGuid))
            return GetTrivialMessage();

        // Try to find in catalog, if not found, return trivial message
        var dsType = _catalog.FindDataSourceInfo(DataSourceGuid, AppId);
        if (dsType == null)
            return GetTrivialMessage();

        // Construct basic options and get the list from the specified data source
        var options = new DataSourceOptions
        {
            AppIdentityOrReader = this.PureIdentity(),
            LookUp = Configuration.LookUpEngine,
        };
        var ds = _dataSourceFactory.Create(dsType.Type, options: options);
        return ds.List;
    }

    private IEnumerable<IEntity> GetTrivialMessage()
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
                { "Name", "Dummy Data from the SystemData DataSource" },
                { nameof(DataSourceGuid), DataSourceGuid }
            }))
        ];

        return l.Return(result, $"{result.Count}");
    }
}