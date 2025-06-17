
// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.Install;

public class InstallAppsDto
{
    public required string remoteUrl { get; init; }

    public required ICollection<AppInstallRuleDto> rules { get; init; }

    public required ICollection<AppDtoLight> installedApps { get; init; }
}

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