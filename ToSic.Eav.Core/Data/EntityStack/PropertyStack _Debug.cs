using ToSic.Eav.Apps;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Data;

partial class PropertyStack
{

    [PrivateApi("Internal")]
    public List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path)
    {
        // No sources - return empty
        if (Sources == null || !Sources.Any()) return [];

        // If path is empty, use Name as base path
        if (string.IsNullOrEmpty(path)) path = NameId ?? "";

        // Get all sources, incl. null-sources
        var sources = SourcesReal
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
                var sourceDump = s.Source._Dump(specs, path);
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
                !AppStackConstants.BlacklistKeys.Any(blk => r.Path.EndsWith(PropertyDumpItem.Separator + blk)))
            .ToArray();

        // V13 - drop null values
        // Edge case where a inherited content-type got more fields but the data hasn't been edited yet
        result = result.Where(r => r.Property.Value != null).ToArray();

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