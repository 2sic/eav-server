using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.LogSettings;

public class AppLoaderLogSettings(ISysFeaturesService featuresSvc) : GetLogSettingsBase(featuresSvc, "Ldr.LogSet")
{
    public ToSic.Sys.Logging.LogSettings GetLogSettings()
        => base.GetLogSettings(
            nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadAppDetails),
            nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadAppSummary)
        );
}