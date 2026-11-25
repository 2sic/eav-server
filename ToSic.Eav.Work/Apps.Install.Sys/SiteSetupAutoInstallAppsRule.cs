using ToSic.Eav.Models;

namespace ToSic.Eav.Apps.Install.Sys;

[ModelSpecs(ContentType = ContentTypeNameId)]
internal record SiteSetupAutoInstallAppsRule : ModelFromEntity
{
    public const string ContentTypeNameId = "833baa25-899b-4242-ade7-323a319bcf71";
    public const string ContentTypeName = "⚙️SiteSetupAutoInstallApps";

    public const string TargetGuid = "guid";
    public const string TargetAll = "all";
    public const string TargetUrl = "url";
    public const string ModeForbidden = "f";
    public const string ModeAllow = "a";
    public const string ModeOptional = "o";
    public const string ModeRequired = "r";

    public string AppGuid => GetThis("");

    public string Mode => GetThis(ModeAllow);

    public string Target => GetThis(TargetGuid);
    
    public string Title => Entity?.GetBestTitle() ?? "";

    public string Url => GetThis("");

    public InstallRuleRaw ToRaw() => new(
        Name: Title,
        AppGuid: AppGuid,
        Mode: Mode,
        Target: Target,
        Url: Url
    );
}
