using ToSic.Eav.Internal.Features;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Loaders;

internal class AppLoaderLoggingHelper(EavFeaturesLoader featuresLoader)
{
    internal LogSettings GetLogSettings()
    {
        var settings = new LogSettings(Details: false);

        var features = featuresLoader.LoadFeaturesStored();
        var featLogging = features?.Features?
            .FirstOrDefault(f => f.Id == BuiltInFeatures.InsightsLoggingCustomized.Guid);

        var config = featLogging?.Configuration;
        if (config == null)
            return settings;

        var detailed = config.ConfigBool(nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataDetailed), false);
        var summary = config.ConfigBool(nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadSystemDataSummary), true);

        settings = settings with { Details = detailed, Summary = summary };

        return settings;
    }

}