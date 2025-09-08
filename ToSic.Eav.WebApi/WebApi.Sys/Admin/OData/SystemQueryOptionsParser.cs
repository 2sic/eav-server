using System.Text.RegularExpressions;

namespace ToSic.Eav.WebApi.Sys.Admin.OData
{
    public static class SystemQueryOptionsParser
    {
        ///// <summary>
        ///// Detect only OData system options that start with $
        ///// </summary>
        //public static bool HasDollarSystemOptions(Uri uri) => Regex.IsMatch(
        //    uri.Query ?? "",
        //    @"(?:^|\?|&)(?:\$|%24)(select|filter|orderby|top|skip|count|expand|search|apply|compute|format|skiptoken|deltatoken)=",
        //    RegexOptions.IgnoreCase);

        /// <summary>
        /// Parse common OData system options from a request URI without building an EDM.
        /// This intentionally does not validate entity or property names.
        /// </summary>
        public static SystemQueryOptions Parse(Uri uri)
        {
            var sys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var custom = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var q = uri.Query ?? string.Empty;
            if (q.StartsWith("?")) q = q.Length > 1 ? q.Substring(1) : string.Empty;

            foreach (var pair in q.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = pair.IndexOf('=');
                string k, v;
                if (eq >= 0)
                {
                    k = Uri.UnescapeDataString(pair.Substring(0, eq));
                    v = Uri.UnescapeDataString(pair.Substring(eq + 1));
                }
                else
                {
                    k = Uri.UnescapeDataString(pair);
                    v = string.Empty;
                }
                if (string.IsNullOrEmpty(k)) continue;

                if (k.StartsWith("$", StringComparison.OrdinalIgnoreCase) || k.StartsWith("%24", StringComparison.OrdinalIgnoreCase))
                    sys[k] = v;
                else
                    custom[k] = v;
            }

            static string? Get(string name, Dictionary<string, string> sys)
            {
                if (sys.TryGetValue("$" + name, out var v1)) return v1;
                if (sys.TryGetValue("%24" + name, out var v2)) return v2;
                return null;
            }

            string? selectRaw = Get("select", sys);
            var selectList = ParseSelect(selectRaw);

            int? AsInt(string? s) => int.TryParse(s, out var i) ? i : null;
            bool? AsBool(string? s) => s == null ? null : (bool.TryParse(s, out var b) ? b : (s == "1" ? true : s == "0" ? false : (bool?)null));

            var result = new SystemQueryOptions(
                Select: selectList,
                Filter: Get("filter", sys),
                OrderBy: Get("orderby", sys),
                Top: AsInt(Get("top", sys)),
                Skip: AsInt(Get("skip", sys)),
                Count: AsBool(Get("count", sys)),
                Expand: Get("expand", sys),
                RawAllSystem: sys,
                Custom: custom
            );

            return result;
        }

        private static IReadOnlyList<string> ParseSelect(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
            var list = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                var c = raw[i];
                if (c == '(') depth++;
                else if (c == ')') depth = Math.Max(0, depth - 1);
                else if (c == ',' && depth == 0)
                {
                    AddSegment(raw.AsSpan(start, i - start), list);
                    start = i + 1;
                }
            }
            if (start < raw.Length) AddSegment(raw.AsSpan(start), list);
            return list;

            static void AddSegment(ReadOnlySpan<char> seg, List<string> list)
            {
                var trimmed = seg.ToString().Trim();
                if (trimmed.Length > 0) list.Add(trimmed);
            }
        }
    }
}
