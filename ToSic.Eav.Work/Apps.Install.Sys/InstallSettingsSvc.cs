using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Sys.PropertyLookup;
using ToSic.Eav.Data.Sys.PropertyStack;
using ToSic.Eav.Models;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Install.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class InstallSettingsSvc(
    LazySvc<IContextOfSiteService> ctxSvc,
    LazySvc<IPlatformAppInstaller> platformAppInstaller,
    LazySvc<ISysFeaturesService> featureService,
    LazySvc<WorkApps> workApps,
    LazySvc<AppDataStackService> appSettingsStack)
    : ServiceBase("Ins.Install", connect: [ctxSvc, platformAppInstaller, featureService, appSettingsStack, workApps])
{
    
    public InstallAppsDto InstallSettings(bool isContentApp)
    {
        var l = Log.Fn<InstallAppsDto>();

        // Get Remote Install URL
        var site = ctxSvc.Value.Context.Site;
        var url = platformAppInstaller.Value
            .GetAutoInstallPackagesUiUrl(isContentApp);

        // Get list of already installed Apps
        var appsOfThisSite = workApps.Value.GetApps(site)
            .Select(a => new AppDtoLight
            {
                name = a.Specs.Name,
                guid = a.Specs.NameId,
                version = a.Specs.VersionSafe(),
            })
            .ToListOpt();

        // Get list of allow/forbid rules for the App installer
        var settingsSources = appSettingsStack.Value
            .InitForPrimaryAppOfZone(site.ZoneId)
            .GetStack(AppStackConstants.Settings);
        var stack = new PropertyStack().Init(AppStackConstants.RootNameSettings, settingsSources);

        var rules = stack.InternalGetPath(
            new PropReqSpecs("SiteSetup.AutoInstallApps", PropReqSpecs.EmptyDimensions, true, Log), new());
        var ruleEntities = rules.Result as IEnumerable<IEntity>; // note: Result is null if nothing found...
        var rulesFinal = ruleEntities?
            .Select(e =>
                e.ToModel<SiteSetupAutoInstallAppsRule>(options: ToModelOptions.DisableTypeNameCheck)!.GetRuleDto())
            .ToListOpt();

        if (!featureService.Value.IsEnabled(BuiltInFeatures.AppAutoInstallerConfigurable.NameId))
        {
            l.A("will not add installer rules as the feature is not enabled");
            rulesFinal = [];
        }

        return l.Return(new()
        {
            remoteUrl = url,
            installedApps = appsOfThisSite,
            rules = rulesFinal,
        });
    }
}