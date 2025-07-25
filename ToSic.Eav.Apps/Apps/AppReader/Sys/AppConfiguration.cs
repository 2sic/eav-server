﻿

// ReSharper disable UnusedMember.Global - we need these, as it's a public API

using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.AppReader.Sys;

/// <summary>
/// The configuration of the app, as you can set it in the app-package definition.
/// </summary>
[PrivateApi("Note: was public till 16.08")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class AppConfiguration(IEntity entity) : EntityBasedType(entity), IAppConfiguration
{
    // todo: probably move most to Eav.Apps.AppConstants
    [PrivateApi] public const string FieldAllowRazor = "AllowRazorTemplates";
    [PrivateApi] public const string FieldAllowToken = "AllowTokenTemplates";
    [PrivateApi] public const string FieldRequiredSxcVersion = "RequiredVersion";
    [PrivateApi] public const string FieldRequiredDnnVersion = "RequiredDnnVersion";
    [PrivateApi] public const string FieldRequiredOqtaneVersion = "RequiredOqtaneVersion";
    [PrivateApi] public const string FieldSupportsAjax = "SupportsAjaxReload";

    public Version Version => GetVersionOrDefault(nameof(Version));

    public string Name => Get(AppLoadConstants.FieldName, EavConstants.NullNameId);

    public string Description => GetThis("");

    public string Folder => GetThis("");

    public bool EnableRazor => Get(FieldAllowRazor, false);

    public bool EnableToken => Get(FieldAllowToken, false);

    public bool IsHidden => Get(AppLoadConstants.FieldHidden, false);

    public bool EnableAjax => Get(FieldSupportsAjax, false);

    public Guid OriginalId => Guid.TryParse(GetThis(""), out var result) ? result : Guid.Empty;

    public Version RequiredSxc => GetVersionOrDefault(FieldRequiredSxcVersion);

    public Version RequiredDnn => GetVersionOrDefault(FieldRequiredDnnVersion);

    public Version RequiredOqtane => GetVersionOrDefault(FieldRequiredOqtaneVersion);

    private Version GetVersionOrDefault(string name) =>
        Version.TryParse(Get(name, ""), out var version) ? version : new();
}