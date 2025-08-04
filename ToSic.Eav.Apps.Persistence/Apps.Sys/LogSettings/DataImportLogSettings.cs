using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.LogSettings;

public class DataImportLogSettings(ISysFeaturesService featuresSvc) : GetLogSettingsBase(featuresSvc, "Ldr.LogSet")
{
    public ToSic.Sys.Logging.LogSettings GetLogSettings()
        => base.GetLogSettings(
            nameof(BuiltInFeatures.InsightsLoggingCustomConfig.ImportDataDetails),
            "dummy-name"
        );
}