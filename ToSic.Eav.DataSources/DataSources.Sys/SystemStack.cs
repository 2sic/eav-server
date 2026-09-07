using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Sys.PropertyDump;
using ToSic.Eav.DataSource.Sys;
using static ToSic.Eav.Apps.Sys.AppStack.AppStackConstants;

namespace ToSic.Eav.DataSources.Sys;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "System Stacks",
    UiHint = "Settings and/or Resources as a Stack",
    Icon = DataSourceIcons.Dns, // todo
    Type = DataSourceType.System,
    NameId = "60806cb1-0c76-4c1e-8dfe-dcec94726f8d",
    NameIds = ["System.SystemStack"],
    Audience = Audience.Advanced,
    DataConfidentiality = DataConfidentiality.Internal,
    ConfigurationType = "f9aca0f0-1b1b-4414-b42e-b337de124124"
    // HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Attributes"
)]
// ReSharper disable once UnusedMember.Global
public class SystemStack: CustomDataSource
{
    #region Configuration

    [Configuration]
    public string? StackNames => Configuration.GetThis();

    [Configuration]
    public string Keys => Configuration.GetThis(fallback: "");

    [Configuration]
    public string View => Configuration.GetThis(fallback: "");

    [Configuration(Fallback = true)]
    public bool AddValues => Configuration.GetThis(true);

    #endregion

    #region Constructor / DI / Services


    public SystemStack(Dependencies services, AppDataStackService dataStackService, IAppReaderFactory appReadFac, IZoneCultureResolver zoneCulture, IPropertyDumpService dumpService)
        : base(services, "Ds.AppStk", connect: [appReadFac, zoneCulture, dataStackService, dumpService])
    {
        ProvideOutRaw(
            () => GetStack(dataStackService, appReadFac, zoneCulture, dumpService),
            options: () => new()
            {
                AppId = AppId,
                RawConvertOptions = new(addKeys: AddValues ? [nameof(AppStackDataRaw.Value)] : null)
            }
        );
    }

    #endregion


    private IImmutableList<AppStackDataRaw> GetStack(
        AppDataStackService dataStackService,
        IAppReaderFactory appReadFac,
        IZoneCultureResolver zoneCulture,
        IPropertyDumpService dumpService)
    {
        Configuration.Parse();

        var appState = appReadFac.Get(this.PureIdentity());

        var languages = zoneCulture.SafeLanguagePriorityCodes();

        var stackName = SystemStackHelpers.GetStackNameOrNull(StackNames) ?? RootNameSettings;
        var viewMixin = GetViewPart(appState, stackName);

        // TODO: option to get multiple stacks /etc.
        // Build Sources List
        var settings = dataStackService.Init(appState).GetStack(stackName, viewMixin);

        // Dump results
        var dump = dumpService.Dump(settings, new("irrelevant", languages, true, Log), "");

        dump = SystemStackHelpers.ApplyKeysFilter(dump, Keys);

        // V1 - show all options, just the top hit
        var res2 = SystemStackHelpers
            .ReducePropertiesToRelevantOnes(dump)
            .ToList();

        return res2
            .Select(r => new AppStackDataRaw(r))
            .ToImmutableOpt();
    }

    private IEntity? GetViewPart(IAppReadEntities appState, string stackName)
    {
        if (!Guid.TryParse(View, out var viewGuid))
            return null;

        var view = appState.List.GetOne(viewGuid)
                   ?? throw new($"Tried to get view but not found. Guid was {viewGuid}");

        return view.Children(stackName).FirstOrDefault();
    }
}
