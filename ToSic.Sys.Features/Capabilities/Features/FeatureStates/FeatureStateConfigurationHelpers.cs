using System.Diagnostics.CodeAnalysis;

namespace ToSic.Sys.Capabilities.Features;

public static class FeatureStateConfigurationHelpers
{

    public static bool ConfigBool(this IDictionary<string, object>? dic, string key, bool fallback = false)
        => (dic?.TryGetValue(key, out var value) ?? false) && value is bool b
            ? b
            : fallback;


    [return: NotNullIfNotNull(nameof(fallback))]
    public static string? ConfigString(this IDictionary<string, object>? dic, string key, string? fallback = default)
        => (dic?.TryGetValue(key, out var value) ?? false) && value is string b
            ? b
            : fallback;

    public static int ConfigInt(this IDictionary<string, object>? dic, string key, int fallback = 0, int? min = default, int? max = default)
    {
        var result = (dic?.TryGetValue(key, out var value) ?? false) && value is int b
            ? b
            : fallback;

        return result < min
            ? min.Value
            : result > max
                ? max.Value
                : result;
    }

    public static bool ConfigBool(this FeatureState fs, string key, bool fallback = false)
        => fs.Configuration.ConfigBool(key, fallback);


    [return: NotNullIfNotNull(nameof(fallback))]
    public static string? ConfigString(this FeatureState fs, string key, string? fallback = default)
        => fs.Configuration.ConfigString(key, fallback);

    public static int ConfigInt(this FeatureState? fs, string key, int fallback = 0, int? min = default, int? max = default)
        => fs?.Configuration.ConfigInt(key, fallback, min, max) ?? fallback;
}