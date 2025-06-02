using ToSic.Eav.Internal.Features;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Internal;

public class AppLoaderLogSettings(ISysFeaturesService featuresSvc): ServiceBase("Ldr.LogSet")
{
    private const string NameDetailed = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadAppDetails);
    private const string NameSummary = nameof(BuiltInFeatures.InsightsLoggingCustomConfig.LoadAppSummary);

    public LogSettings GetLogSettings()
    {
        var l = Log.Fn<LogSettings>();
        var settings = new LogSettings(Details: false);

        var config = featuresSvc.Get(BuiltInFeatures.InsightsLoggingCustomized.NameId)
            ?.Configuration;

        if (config == null)
            return l.Return(settings, "no configuration");

        settings = PatchLogSettings(settings, config, NameDetailed, NameSummary);

        return l.Return(settings, "with changed configuration");
    }

    public static LogSettings PatchLogSettings(LogSettings settings, Dictionary<string, object> config, string nameDetailed, string nameSummary)
    {
        var detailed = config.ConfigBool(nameDetailed, settings.Details);
        var summary = config.ConfigBool(nameSummary, settings.Summary);

        settings = settings with
        {
            Details = detailed,
            Summary = summary
        };
        return settings;
    }
}