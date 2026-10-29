using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.LogSettings;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataImportLogSettings(ISysFeaturesService featuresSvc) : ServiceBase("Ldr.LogSet")
{
    public ToSic.Sys.Logging.LogSettings GetLogSettings()
        => featuresSvc.GetLogSettings(
            nameof(BuiltInFeatures.InsightsLoggingCustomConfig.ImportDataDetails),
            "dummy-name",
            Log
        );
}