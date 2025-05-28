using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Internal.Features;

namespace ToSic.Eav.Internal.Loaders;

internal class EavSystemLoaderLogSettingsHelper(EavFeaturesLoader featuresLoader, ILog parentLog): HelperBase(parentLog, "Ldr.LogSet")
{
    private const string NameDetailed = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataDetails);
    private const string NameSummary = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataSummary);

    internal LogSettings GetLogSettings()
    {
        var l = Log.Fn<LogSettings>();
        var settings = new LogSettings(Details: false);

        var features = featuresLoader.LoadFeaturesStored();
        var config = features
            ?.Features
            ?.FirstOrDefault(f => f.Id == BuiltInFeatures.InsightsLoggingCustomized.Guid)
            ?.Configuration;

        if (config == null)
            return l.Return(settings, "no configuration");

        settings = AppLoaderLogSettings.PatchLogSettings(settings, config, NameDetailed, NameSummary);

        return l.Return(settings, "with changed configuration");
    }
}