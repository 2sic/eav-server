namespace ToSic.Eav.WebApi.Sys.Admin.OData
{
    /// <summary>
    /// Lightweight, parser for a subset of OData system query options.
    /// </summary>
    public static class SystemQueryOptionsParser
    {
        // Defensive limits (intentionally generous; adjust if needed). They only affect extreme / abusive inputs.
        private const int MaxParameterCount = 500;      // Prevent &x=... repeated abuse
        private const int MaxValueLength    = 8192;     // Cap any single value size
        private const int MaxSelectItems    = 200;      // Prevent huge $select lists
        private const int MaxSelectDepth    = 64;       // Prevent pathological parenthesis nesting

        /// <summary>
        /// Parse common OData system options from a request URI without building an EDM.
        /// This intentionally does not validate entity or property names.
        /// </summary>
        public static SystemQueryOptions Parse(Uri uri)
        {
            var sys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var custom = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var q = uri.Query ?? string.Empty;
            if (q.Length > 0 && q[0] == '?') q = q.Length > 1 ? q.Substring(1) : string.Empty;
            if (string.IsNullOrEmpty(q))
                return EmptyResult(sys, custom);

            // Streaming parse instead of string.Split to avoid large temporary arrays under heavy input.
            int index = 0;
            int paramCount = 0;
            while (index <= q.Length)
            {
                if (paramCount >= MaxParameterCount) break; // stop processing more parameters
                int nextAmp = q.IndexOf('&', index);
                if (nextAmp < 0) nextAmp = q.Length;
                int length = nextAmp - index;
                if (length > 0)
                {
                    var pairSpan = q.AsSpan(index, length);
                    int eq = pairSpan.IndexOf('=');
                    ReadOnlySpan<char> rawKeySpan, rawValueSpan;
                    if (eq >= 0)
                    {
                        rawKeySpan = pairSpan.Slice(0, eq);
                        rawValueSpan = pairSpan.Slice(eq + 1);
                    }
                    else
                    {
                        rawKeySpan = pairSpan;
                        rawValueSpan = ReadOnlySpan<char>.Empty;
                    }

                    if (!rawKeySpan.IsEmpty)
                    {
                        var rawKey = rawKeySpan.ToString();
                        var rawValue = rawValueSpan.ToString();

                        // Safe unescape: never throw on malformed % sequences; fall back to raw.
                        var k = SafeUnescape(rawKey).Trim();
                        if (k.Length > 0)
                        {
                            var v = SafeUnescape(rawValue);
                            if (v.Length > MaxValueLength)
                                v = v.Substring(0, MaxValueLength);

                            if (k.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                                sys[k] = v; // last wins
                            else
                                custom[k] = v;
                        }
                    }
                    paramCount++;
                }
                index = nextAmp + 1; // move past '&' (or q.Length to exit)
                if (index > q.Length) break;
            }

            static string? Get(string name, Dictionary<string, string> sys)
                => sys.TryGetValue("$" + name, out var v1) ? v1 : null;

            var selectRaw = Get("select", sys);
            var selectList = ParseSelect(selectRaw);

            int? AsInt(string? s) => int.TryParse(s, out var i) ? i : null;
            bool? AsBool(string? s) => s == null ? null : (bool.TryParse(s, out var b) ? b : (s == "1" ? true : s == "0" ? false : (bool?)null));

            return new SystemQueryOptions(
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
        }

        private static SystemQueryOptions EmptyResult(
            IReadOnlyDictionary<string, string> sys,
            IReadOnlyDictionary<string, string> custom)
            => new(
                Select: Array.Empty<string>(),
                Filter: null,
                OrderBy: null,
                Top: null,
                Skip: null,
                Count: null,
                Expand: null,
                RawAllSystem: sys,
                Custom: custom);

        private static string SafeUnescape(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            try { return Uri.UnescapeDataString(input); }
            catch (UriFormatException) { return input; } // Fallback: treat as already unescaped / raw
        }

        private static IReadOnlyList<string> ParseSelect(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
            var list = new List<string>();
            int depth = 0;
            int start = 0;
            int items = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                var c = raw[i];
                if (c == '(')
                {
                    if (depth < MaxSelectDepth) depth++;
                }
                else if (c == ')')
                {
                    if (depth > 0) depth--; // clamp
                }
                else if (c == ',' && depth == 0)
                {
                    if (!AddSegment(raw.AsSpan(start, i - start), list)) return list; // limit reached
                    items++;
                    if (items >= MaxSelectItems) return list; // stop early
                    start = i + 1;
                }
            }
            if (start < raw.Length) AddSegment(raw.AsSpan(start), list);
            return list;

            static bool AddSegment(ReadOnlySpan<char> seg, List<string> list)
            {
                var trimmed = seg.ToString().Trim();
                if (trimmed.Length > 0) list.Add(trimmed);
                return true;
            }
        }
    }
}
