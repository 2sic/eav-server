namespace ToSic.Eav.SysData;

internal static class FeatureStateConfigurationHelpers
{
    public static bool ConfigBool(this FeatureState fs, string key, bool fallback = false) =>
        (fs.Configuration?.TryGetValue(key, out var value) ?? false) && value is bool b
            ? b
            : fallback;


    public static string ConfigString(this FeatureState fs, string key, string fallback = default) =>
        (fs.Configuration?.TryGetValue(key, out var value) ?? false) && value is string b
            ? b
            : fallback;

    public static int ConfigInt(this FeatureState fs, string key, int fallback = 0) =>
        (fs.Configuration?.TryGetValue(key, out var value) ?? false) && value is int b
            ? b
            : fallback;

}