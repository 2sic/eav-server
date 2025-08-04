using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.LogSettings;

public abstract class GetLogSettingsBase(ISysFeaturesService featuresSvc, string logName): ServiceBase(logName)
{
    protected ToSic.Sys.Logging.LogSettings GetLogSettings(string nameDetailed, string nameSummary)
    {
        var l = Log.Fn<ToSic.Sys.Logging.LogSettings>();
        var settings = new ToSic.Sys.Logging.LogSettings(Details: false);

        var config = featuresSvc.Get(BuiltInFeatures.InsightsLoggingCustomized.NameId)
            ?.Configuration;

        if (config == null)
            return l.Return(settings, "no configuration");

        settings = PatchLogSettings(settings, config, nameDetailed, nameSummary);

        return l.Return(settings, "with changed configuration");
    }


    public static ToSic.Sys.Logging.LogSettings PatchLogSettings(ToSic.Sys.Logging.LogSettings settings, Dictionary<string, object> config, string nameDetailed, string nameSummary)
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