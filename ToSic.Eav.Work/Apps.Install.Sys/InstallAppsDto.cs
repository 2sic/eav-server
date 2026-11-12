
// ReSharper disable InconsistentNaming

namespace ToSic.Eav.Apps.Install.Sys;

// TODO: @2rb
// These are only used between the service and the data-source.
// Change the InstallSettingsSvc to already return the raw data, so that these DTOs can be removed 
// and there is no additional conversion.

public class InstallAppsDto
{
    public required string? remoteUrl { get; init; }

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