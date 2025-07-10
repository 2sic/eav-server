using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.LogSettings;

/// <summary>
/// The System Load Log Settings are different from all others,
/// because they must be parsed before the normal log services are ready.
///
/// Do not use this for other log settings, but instead use the other classes.
/// </summary>
internal class EavFeaturesLogSettingsHelper(EavFeaturesLoader featuresLoader, ILog parentLog): HelperBase(parentLog, "Ldr.LogSet")
{
    private const string NameDetailed = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataDetails);
    private const string NameSummary = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataSummary);

    internal ToSic.Sys.Logging.LogSettings GetLogSettings()
    {
        var l = Log.Fn<ToSic.Sys.Logging.LogSettings>();

        var features = featuresLoader
            .LoadFeaturesStored()
            ?.Features;
        var config = features
            ?.FirstOrDefault(f => f.Id == BuiltInFeatures.InsightsLoggingCustomized.Guid)
            ?.Configuration;

        if (config == null)
            return l.Return(new(Details: false), "no configuration");

        // temp check if optimized patching works
        var stateLinqOptimized = features?.FirstOrDefault(f => f.Id == BuiltInFeatures.LinqListOptimizations.Guid);

        if (stateLinqOptimized?.Enabled == true)
        {
            l.A($"WIP: enable optimized LINQ");
            BuiltInFeatures.LinqListOptimizations.RunOnStateChange?.Invoke(new(BuiltInFeatures.LinqListOptimizations, true), l);
        }

        var settings = GetLogSettingsBase.PatchLogSettings(new(Details: false), config, NameDetailed, NameSummary);

        return l.Return(settings, "with changed configuration");
    }
}