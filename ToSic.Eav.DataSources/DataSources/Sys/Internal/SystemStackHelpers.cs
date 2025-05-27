using ToSic.Eav.Data.Debug;
using ToSic.Eav.Plumbing;
using ToSic.Sys.Utils;
using static ToSic.Eav.Apps.AppStackConstants;

namespace ToSic.Eav.DataSources.Sys.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class SystemStackHelpers
{
    public static string GetStackNameOrNull(string part)
    {
        // Ensure name is known
        if (RootNameSettings.EqualsInsensitive(part)) return RootNameSettings;
        if (RootNameResources.EqualsInsensitive(part)) return RootNameResources;
        return null;
    }



    public static List<PropertyDumpItem> ApplyKeysFilter(List<PropertyDumpItem> results, string key)
    {
        if (string.IsNullOrEmpty(key)) return results;

        var keyList = key.CsvToArrayWithoutEmpty();
        if (!keyList.Any()) return results;

        var relevant = results
            .Where(r => keyList.Any(k => r.Path.EqualsInsensitive(k)))
            .ToList();
        return relevant.SelectMany(r => r.AllOptions).ToList();
    }

    public static IEnumerable<PropertyDumpItem> ReducePropertiesToRelevantOnes(List<PropertyDumpItem> results)
    {
        var final = results
            .GroupBy(original => new { original.Path, original.SourceName }) // remove "duplicate" settings from results
            .Select(g => g.OrderByDescending(i => i.AllOptions?.Count ?? 0).First());
        return final;
    }

}