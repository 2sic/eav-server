using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.LogSettings;


[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogSettingsExtensions
{
    internal static ToSic.Sys.Logging.LogSettings GetLogSettings(this ISysFeaturesService featuresSvc, string nameDetailed, string nameSummary, ILog Log)
    {
        var l = Log.Fn<ToSic.Sys.Logging.LogSettings>();
        var settings = new ToSic.Sys.Logging.LogSettings(Details: false);

        var config = featuresSvc.Get(BuiltInFeatures.InsightsLoggingCustomized.NameId)
            ?.Configuration;

        if (config == null)
            return l.Return(settings, "no configuration");

        settings = settings.PatchLogSettings(config, nameDetailed, nameSummary);

        return l.Return(settings, "with changed configuration");
    }

    public static ToSic.Sys.Logging.LogSettings PatchLogSettings(this ToSic.Sys.Logging.LogSettings settings, Dictionary<string, object> config, string nameDetailed, string nameSummary)
    {
        return settings with
        {
            Details = config.ConfigBool(nameDetailed, fallback: settings.Details),
            Summary = config.ConfigBool(nameSummary, fallback: settings.Summary)
        };
    }
}