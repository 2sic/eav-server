using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.Data.PropertyDump.Sys;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSources.Sys.Internal;
using static ToSic.Eav.Apps.Sys.AppStack.AppStackConstants;

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
// ReSharper disable once UnusedMember.Global
public class SystemStack: CustomDataSourceAdvanced
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

    private readonly IAppReaderFactory _appReadFac;
    private readonly IZoneCultureResolver _zoneCulture;
    private readonly IPropertyDumpService _dumpService;
    private readonly AppDataStackService _dataStackService;

    public SystemStack(MyServices services, AppDataStackService dataStackService, IAppReaderFactory appReadFac, IZoneCultureResolver zoneCulture, IPropertyDumpService dumpService)
        : base(services, "Ds.AppStk", connect: [appReadFac, zoneCulture, dataStackService, dumpService])
    {
        _appReadFac = appReadFac;
        _zoneCulture = zoneCulture;
        _dumpService = dumpService;
        _dataStackService = dataStackService;
        ProvideOut(GetStack);
    }

    #endregion


    private IImmutableList<IEntity> GetStack()
    {
        Configuration.Parse();

        var appState = _appReadFac.Get(this.PureIdentity());

        var languages = _zoneCulture.SafeLanguagePriorityCodes();

        var stackName = SystemStackHelpers.GetStackNameOrNull(StackNames) ?? RootNameSettings;

        // TODO: option to get multiple stacks /etc.
        // Build Sources List
        var settings = _dataStackService.Init(appState).GetStack(stackName);

        // Dump results
        var dump = _dumpService.Dump(settings, new(null, languages, true, Log), null);

        dump = SystemStackHelpers.ApplyKeysFilter(dump, Keys);

        // V1 - show all options, just the top hit
        var res2 = SystemStackHelpers
            .ReducePropertiesToRelevantOnes(dump)
            .ToList();

        var asRaw = res2
            .Select(r => new AppStackDataRaw(r))
            .ToList();
        // Note: must use configure here, because AppId and AddValues are properties that's not set in the constructor
        var options = new RawConvertOptions(addKeys: AddValues ? new[] { "Value" } : null);
        var stackFactory = DataFactory.SpawnNew(options: AppStackDataRaw.Options with { AppId = AppId, RawConvertOptions = options });
        var converted = stackFactory.Create(asRaw);

        return converted;
    }
}