using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSources.Sys.Internal;
using static ToSic.Eav.Apps.AppStackConstants;

namespace ToSic.Eav.DataSources.Sys;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "System Stacks",
    UiHint = "Settings and/or Resources as a Stack",
    Icon = DataSourceIcons.Dns, // todo
    Type = DataSourceType.System,
    NameId = "60806cb1-0c76-4c1e-8dfe-dcec94726f8d",
    Audience = Audience.Advanced,
    ConfigurationType = "f9aca0f0-1b1b-4414-b42e-b337de124124"
    // HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Attributes"
)]
public class SystemStack: Eav.DataSource.DataSourceBase
{
    #region Configuration

    [Configuration]
    public string StackNames => Configuration.GetThis();

    [Configuration]
    public string Keys => Configuration.GetThis();

    [Configuration(Fallback = true)]
    public bool AddValues => Configuration.GetThis(true);

    #endregion

    #region Constructor / DI / Services

    private readonly IDataFactory _dataFactory;
    private readonly IAppStates _appStates;
    private readonly IZoneCultureResolver _zoneCulture;
    private readonly AppDataStackService _dataStackService;

    public SystemStack(MyServices services,
        AppDataStackService dataStackService,
        IAppStates appStates,
        IZoneCultureResolver zoneCulture,
        IDataFactory dataFactory
    ) : base(services, "Ds.AppStk")
    {
        ConnectServices(
            _appStates = appStates,
            _zoneCulture = zoneCulture,
            _dataStackService = dataStackService,
            _dataFactory = dataFactory
        );
        ProvideOut(GetStack);
    }

    #endregion


    private IImmutableList<IEntity> GetStack()
    {
        Configuration.Parse();

        var appState = _appStates.GetReader(this);

        var languages = _zoneCulture.SafeLanguagePriorityCodes();

        var stackName = SystemStackHelpers.GetStackNameOrNull(StackNames) ?? RootNameSettings;

        // TODO: option to get multiple stacks /etc.
        // Build Sources List
        var settings = _dataStackService.Init(appState).GetStack(stackName);

        // Dump results
        var dump = settings._Dump(new PropReqSpecs(null, languages, Log), null);

        dump = SystemStackHelpers.ApplyKeysFilter(dump, Keys);

        // V1 - show all options, just the top hit
        var res2 = SystemStackHelpers.ReducePropertiesToRelevantOnes(dump)
            .ToList();

        var asRaw = res2.Select(r => new AppStackDataRaw(r)).ToList();
        // Note: must use configure here, because AppId and AddValues are properties that's not set in the constructor
        var options = new RawConvertOptions(addKeys: AddValues ? new[] { "Value" } : null);
        var stackFactory = _dataFactory.New(options: new DataFactoryOptions(AppStackDataRaw.Options, appId: AppId), rawConvertOptions: options);
        var converted = stackFactory.Create(asRaw);

        return converted;
    }
}