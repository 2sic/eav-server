using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Sys.PropertyStack;
using static System.String;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Settings;

internal class LoadSettingsForContentType()
    : ServiceBase($"Api.LdStCT"), ILoadSettingsProvider
{
    public Dictionary<string, object> GetSettings(LoadSettingsProviderParameters parameters)
    {
        var l = Log.Fn<Dictionary<string, object>>();
        // find all keys which may be necessary
        var settingsKeys = parameters.ContentTypes
            .SelectMany(ct =>
                (ct.DetailsOrNull()?.AdditionalSettings ?? "").CsvToArrayWithoutEmpty()
            )
            .Where(c => !IsNullOrWhiteSpace(c))
            // Only include settings which have the full path
            // so in future we can add other roots like resources
            .Where(s => s.StartsWith($"{AppStackConstants.RootNameSettings}."))
            .ToList();

        // Try to find each setting
        var settings = SettingsByKeys(parameters.ContextOfApp.AppSettings, settingsKeys);

        return l.Return(settings, $"{settings.Count}");
    }
    protected Dictionary<string, object> SettingsByKeys(PropertyStack appSettings, List<string> keys)
    {
        var l = Log.Fn<Dictionary<string, object>>();
        // Try to find each setting
        var settings = keys.ToDictionary(
            key => key,
            key => appSettings.InternalGetPath(key).Result!
        );

        return l.Return(settings, $"{settings.Count}");
    }

}