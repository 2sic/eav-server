using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.DataSource.Sys.Catalog;
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
public sealed class SysData(CustomDataSource.Dependencies services, DataSourceCatalog catalog, IDataSourcesService dataSourceFactory, IUser user)
    : CustomDataSource(services, $"{DataSourceConstantsInternal.LogPrefix}.SysDat", connect: [catalog, dataSourceFactory])
{
    #region Configuration-properties

    /// <summary>
    /// The NameId of the DataSource to get data from.
    /// </summary>
    /// <remarks>
    /// It uses a fairly exotic name to avoid conflicts with parameter names of the data-sources being called.
    /// </remarks>
    [Configuration(Fallback = "", Token = $"[{ParamsSourceName}:{nameof(SysDataSource)}]")]
    public string SysDataSource => Configuration.GetThis(fallback: "");

    ///// <summary>
    ///// The NameId of the DataSource to get data from.
    ///// </summary>
    ///// <remarks>
    ///// It uses a fairly exotic name to avoid conflicts with parameter names of the data-sources being called.
    ///// </remarks>
    //[Configuration(Fallback = StreamDefaultName, Token = $"[{ParamsSourceName}:{nameof(SysDataStream)}||{StreamDefaultName}]")]
    //public string SysDataStream => Configuration.GetThis(fallback: StreamDefaultName);

    #endregion

    public override IReadOnlyDictionary<string, IDataStream> Out => field ??= GetDataSource().Out;

    private IDataSource GetDataSource()
    {
        var inStreamName = StreamDefaultName;

        // If nothing relevant specified, return trivial message
        if (string.IsNullOrWhiteSpace(SysDataSource))
            return GetTrivialMessageDs(false, inStreamName, false);

        // Try to find in catalog, if not found, return trivial message
        var dsInfo = catalog.FindDataSourceInfo(SysDataSource, AppId);
        if (dsInfo == null)
            return GetTrivialMessageDs(false, inStreamName, false);

        if (!dsInfo.IsAllowed(user))
            return GetTrivialMessageDs(true, inStreamName, false, $"Not allowed, DataConfidentiality: {dsInfo.VisualQuery?.DataConfidentiality}");

        // Construct basic options and build the source

        var options = CreateInnerOptions();
        return dataSourceFactory.Create(dsInfo.Type, options: options);
    }

    private DataSourceOptions CreateInnerOptions() =>
        new()
        {
            AppIdentityOrReader = this.PureIdentity(),
            LookUp = CreateLookUpEngine(),
        };


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

    private IDataSource GetTrivialMessageDs(bool dsFound, string streamName, bool streamFound, string? allowed = default)
    {
        var msg = GetTrivialMessage(dsFound, streamName, streamFound, allowed);
        var ds = dataSourceFactory.Create<Error>(options: CreateInnerOptions());
        ds.UseCustomErrorData(msg);
        return ds;
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