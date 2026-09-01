using System.Diagnostics.CodeAnalysis;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Features;

static class FeatureStateTestAccessors
{
    public static bool ConfigBoolTac(this IDictionary<string, object>? dic, string key, bool fallback = false)
        => dic.ConfigBool(key, fallback);

    public static bool ConfigBoolTac(this FeatureState fs, string key, bool fallback = false)
        => fs.ConfigBool(key, fallback);


    public static int ConfigIntTac(this FeatureState? fs, string key, int fallback = 0, int? min = default, int? max = default)
        => fs.ConfigInt(key, fallback, min, max);


    [return: NotNullIfNotNull(nameof(fallback))]
    public static string? ConfigStringTac(this FeatureState fs, string key, string? fallback = default)
        => fs.ConfigString(key, fallback);

}