using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.PropertyDump.Sys;

public class PropertyStackDump: IPropertyDumper
{
    #region Internal Constants for filtering out some keys, seem to be so that debug info hides confusing properties...

    private const string SysSettingsFieldScope = "SettingsEntityScope";

    private static string FieldSettingsIdentifier = "SettingsIdentifier";
    private static string FieldItemIdentifier = "ItemIdentifier";

    private static string[] BlacklistKeys = [FieldSettingsIdentifier, FieldItemIdentifier, SysSettingsFieldScope];

    #endregion


    public int IsCompatible(object target)
        => target is PropertyStack.Sys.PropertyStack ? 100 : 0;

    public List<PropertyDumpItem> Dump(object stack, PropReqSpecs specs, string path, IPropertyDumpService dumpService)
        => DumpTyped((PropertyStack.Sys.PropertyStack)stack, specs, path, dumpService);

    private List<PropertyDumpItem> DumpTyped(PropertyStack.Sys.PropertyStack stack, PropReqSpecs specs, string path, IPropertyDumpService dumpService)
    {
        // No sources - return empty
        if (stack.Sources.SafeNone())
            return [];

        // If path is empty, use Name as base path
        if (string.IsNullOrEmpty(path))
            path = stack.NameId ?? "";

        // Get all sources, incl. null-sources
        var sources = stack.SourcesReal
            .Select((s, i) => new
            {
                s.Key, 
                Source = s.Value, 
                Index = i
            })
            .ToList();

        var result = sources
            .SelectMany(s =>
            {
                var sourceDump =
                    dumpService?.Dump(s.Source, specs, path)
                    ?? [];
                    // #DropUseOfDumpProperties
                    //?? s.Source._DumpNameWipDroppingMostCases(specs, path);
                sourceDump.ForEach(sd =>
                {
                    sd.SourceName = s.Key;
                    sd.SourcePriority = s.Index;
                });
                return sourceDump;
            })
            .ToArray();

        // Remove settings-internal keys which are not useful
        // use Blacklist to find these
        result = result.Where(r =>
                !BlacklistKeys.Any(blk => r.Path.EndsWith(PropertyDumpItem.Separator + blk)))
            .ToArray();

        // V13 - drop null values
        // Edge case where a inherited content-type got more fields but the data hasn't been edited yet
        result = result
            .Where(r => r.Property.Value != null)
            .ToArray();

        var grouped = result
            .OrderBy(r => r.Path)
            .ThenBy(r => r.SourcePriority)
            .GroupBy(r => r.Path)
            .ToArray();

        var bestMatches = grouped
            .Select(g =>
            {
                var top = g.First();
                top.AllOptions = g.Select(i => i).ToList();
                return top;
            })
            .ToList();

        return bestMatches;
    }

}