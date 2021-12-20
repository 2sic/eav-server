using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    [PrivateApi("Hide implementation")]
    public partial class PropertyStack
    {

        [PrivateApi("Internal")]
        public List<PropertyDumpItem> _Dump(string[] languages, string path, ILog parentLogOrNull)
        {
            // No sources - return empty
            if (Sources == null || !Sources.Any()) return new List<PropertyDumpItem>();

            // If path is empty, use Name as base path
            if (string.IsNullOrEmpty(path)) path = Name ?? "";

            var sources = SourcesReal
                // .Where(s => s.Value != null)
                .Select((s, i) => new
                {
                    s.Key, 
                    Source = s.Value, 
                    Index = i
                });
            var result = sources
                .SelectMany(s =>
                {
                    var sourceDump = s.Source._Dump(languages, path, parentLogOrNull);
                    sourceDump.ForEach(sd =>
                    {
                        sd.SourceName = s.Key;
                        sd.SourcePriority = s.Index;
                    });
                    return sourceDump;
                });

            // Remove settings-internal keys which are not useful
            // use Blacklist to find these
            result = result.Where(r =>
                !ConfigurationConstants.BlacklistKeys.Any(blk => r.Path.EndsWith(PropertyDumpItem.Separator + blk)));

            var grouped = result
                .OrderBy(r => r.Path)
                .ThenBy(r => r.SourcePriority)
                .GroupBy(r => r.Path);

            var bestMatches = grouped
                .Select(g =>
                {
                    var top = g.First();
                    top.AllOptions = g.Select(i => i).ToList();
                    return top;
                });

            return bestMatches.ToList();
        }
        
    }
}
