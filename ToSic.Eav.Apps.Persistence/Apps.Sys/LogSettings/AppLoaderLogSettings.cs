using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.LogSettings;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppLoaderLogSettings(ISysFeaturesService featuresSvc) : ServiceBase("Ldr.LogSet")
{
    public ToSic.Sys.Logging.LogSettings GetLogSettings()
        => featuresSvc.GetLogSettings(
            nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadAppDetails),
            nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadAppSummary),
            Log
        );
}