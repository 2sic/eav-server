using ToSic.Eav.Context;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.Apps.Inspect.Sys;

[PrivateApi]
[VisualQuery(
    NiceName = "Inheritable Apps",
    NameId = "64b81dd8-23b6-4a37-bc43-78661e3a3e2c",
    NameIds = ["System.AppsInheritable"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System,
    UiHint = "Apps from other sites which the current site can inherit")]
[ShowApiWhenReleased(ShowApiMode.Never)]
// ReSharper disable once UnusedMember.Global
public class InheritableApps(CustomDataSource.Dependencies services, IAppDetailsService appsInfoSvc, ISite site, WorkAppsInheritable workApps)
    : CustomDataSource(services, logName: "Sxc.InhApps", connect: [appsInfoSvc, site, workApps])
{
    protected override IEnumerable<IRawData> GetDefault()
        => workApps.GetInheritableApps(site)
            .Select(app => appsInfoSvc.GetDetails(site, app))
            .ToListOpt();
}
