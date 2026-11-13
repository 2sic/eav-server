using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.Apps.Install.Sys;

internal sealed record InstallAppsRaw(
    InstallationSettingsRaw Settings,
    IReadOnlyCollection<InstalledAppRaw> InstalledApps,
    IReadOnlyCollection<InstallRuleRaw> Rules
);

internal sealed record InstallationSettingsRaw(string? RemoteUrl) : IRawEntityAutoConvert;

internal sealed record InstalledAppRaw(
    [property: ContentTypeTitle] string Name,
    string AppGuid,
    string Version
) : IRawEntityAutoConvert;

internal sealed record InstallRuleRaw(
    [property: ContentTypeTitle] string Name,
    string AppGuid,
    string Mode,
    string Target,
    string Url
) : IRawEntityAutoConvert;
