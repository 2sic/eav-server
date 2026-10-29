namespace ToSic.Eav.Apps.Sys.Stack;

/// <summary>
/// Identifiers (ContentType Names) of Settings/Resources in different scenarios.
/// Mainly for the edit-UI to load the expected content type in various situations.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public struct AppThingsIdentifiers
{
    /// <summary>
    /// This says if the identifiers are for the Settings or Resources stack
    /// </summary>
    public AppThingsToStack Target;

    /// <summary>
    /// This is the content type name used on all Apps for the system settings or resources. For example "SettingsSystem" or "ResourcesSystem"
    /// </summary>
    public string SystemType;

    /// <summary>
    /// This is the content type name used on the Primary or Global App for the custom settings or resources. For example "SettingsCustom" or "ResourcesCustom"
    /// </summary>
    /// <remarks>
    /// The UI will offer this content-type for editing on the primary or global app only.
    /// Other apps will use the `AppType` below.
    ///
    /// This was rediscovered 2026-09-15 by 2dm.
    /// It's not quite sure why we did this a LONG time ago, but I believe the reason was
    /// that the name "App-Settings" etc. has hyphens - which are totally non-standard, and we wanted to migrate to this instead.
    /// Note that this content-type is NOT auto-generated, but will be generated on demand, if an admin starts to configure the fields of a Primary/Global app.
    /// This is so rare, that it's not even regularly tested.
    /// </remarks>
    public string CustomType;

    /// <summary>
    /// This is the content type name used on normal apps (Content, Blog, etc.) for the settings or resources. For example "App-Settings" or "App-Resources"
    /// </summary>
    public string AppType;
}