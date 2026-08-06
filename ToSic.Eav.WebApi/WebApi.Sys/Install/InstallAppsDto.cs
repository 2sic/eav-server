
// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.Install;

// TODO: @2rb
// probably just rename these to ...Raw
// and add [ContentType] etc. with new random guids https://guidgenerator.com/

public class InstallAppsDto
{
    public required string remoteUrl { get; init; }

    public required ICollection<AppInstallRuleDto>? rules { get; init; }

    public required ICollection<AppDtoLight> installedApps { get; init; }
}

// TODO: @2rb - this already seems to have a RAW: SiteSetupAutoInstallAppsRule
// probably use that...

public class AppInstallRuleDto
{
    public required string name { get; init; }
    public required string appGuid { get; init; }
    public required string mode { get; init; }
    public required string target { get; init; }
    public required string url { get; init; }
}

public class AppDtoLight
{
    public required string name { get; init; }
    public required string guid { get; init; }
    public required string version { get; init; }
}