using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.Apps.Install.Sys;

[PrivateApi]
[VisualQuery(
    NiceName = "App Installation",
    NameId = "187dfef4-13a5-4aed-a806-f5756f424176",
    NameIds = ["System.AppInstallation"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System,
    UiHint = "Settings and state used by the app installer")]
public class AppInstallation : CustomDataSource
{
    [Configuration]
    public bool IsContentApp => Configuration.GetThis(false);

    public AppInstallation(Dependencies services, LazySvc<InstallSettingsSvc> install)
        : base(services, "Sxc.AppInstall", connect: [install])
    {
        var result = new Lazy<InstallAppsRaw>(() => install.Value.InstallSettings(IsContentApp));

        ProvideOutRaw(name: "Settings", data: () => new[] { result.Value.Settings }, options: Options);
        ProvideOutRaw(name: "InstalledApps", data: () => result.Value.InstalledApps, options: Options);
        ProvideOutRaw(name: "Rules", data: () => result.Value.Rules, options: Options);
    }

    private static DataFactoryOptions Options() => new();
}
