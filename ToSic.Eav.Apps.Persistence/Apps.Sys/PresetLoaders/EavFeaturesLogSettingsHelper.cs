using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.PresetLoaders;

internal class EavFeaturesLogSettingsHelper(EavFeaturesLoader featuresLoader, ILog parentLog): HelperBase(parentLog, "Ldr.LogSet")
{
    private const string NameDetailed = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataDetails);
    private const string NameSummary = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataSummary);

    internal LogSettings GetLogSettings()
    {
        var l = Log.Fn<LogSettings>();

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

        var settings = AppLoaderLogSettings.PatchLogSettings(new(Details: false), config, NameDetailed, NameSummary);

        return l.Return(settings, "with changed configuration");
    }
}