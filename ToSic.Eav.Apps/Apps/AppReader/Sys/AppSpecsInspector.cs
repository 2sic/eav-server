using System.Text.RegularExpressions;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Sys;

namespace ToSic.Eav.Apps.Internal;

public static class AppSpecsInspector
{
    public static bool IsContentApp(this IAppSpecs specs)
        => specs.NameId == KnownAppsConstants.DefaultAppGuid;

    // TODO: @STV - try to use this where possible
    public static bool IsSiteSettingsApp(this IAppSpecs specs)
        => specs.NameId == KnownAppsConstants.PrimaryAppGuid;

    public static string VersionSafe(this IAppSpecs app)
        => app.Configuration.Version?.ToString() ?? "";

    public static string NameWithoutSpecialChars(this IAppSpecs app)
        => Regex.Replace(app.Name, "[^a-zA-Z0-9-_]", "");

    public static string ToFileNameWithVersion(this IAppSpecs specs)
        => $"{specs.NameWithoutSpecialChars()}_{specs.VersionSafe()}";
}