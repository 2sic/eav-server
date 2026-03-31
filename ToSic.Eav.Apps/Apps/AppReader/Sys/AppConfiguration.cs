using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.AppReader.Sys;

/// <summary>
/// The configuration of the app, as you can set it in the app-package definition.
/// </summary>
[PrivateApi("Note: was public till 16.08")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal record AppConfiguration : ModelFromEntityBasic, IAppConfiguration
{
    public Version Version => GetVersionOrDefault(nameof(Version));

    public string Name => Get(AppConfigurationFields.FieldName, EavConstants.NullNameId);

    public string Description => GetThis("");

    public string Folder => GetThis("");

    public bool EnableRazor => Get(AppConfigurationFields.FieldAllowRazor, false);

    public bool EnableToken => Get(AppConfigurationFields.FieldAllowToken, false);

    public bool IsHidden => Get(AppLoadConstants.FieldHidden, false);

    public bool EnableAjax => Get(AppConfigurationFields.FieldSupportsAjax, false);

    public Guid OriginalId => Guid.TryParse(GetThis(""), out var result) ? result : Guid.Empty;

    public Version RequiredSxc => GetVersionOrDefault(AppConfigurationFields.FieldRequiredSxcVersion);

    public Version RequiredDnn => GetVersionOrDefault(AppConfigurationFields.FieldRequiredDnnVersion);

    public Version RequiredOqtane => GetVersionOrDefault(AppConfigurationFields.FieldRequiredOqtaneVersion);

    private Version GetVersionOrDefault(string name) =>
        Version.TryParse(Get(name, ""), out var version) ? version : new();
}