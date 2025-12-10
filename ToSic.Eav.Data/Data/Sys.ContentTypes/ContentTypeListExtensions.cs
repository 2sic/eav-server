namespace ToSic.Eav.Data.Sys.ContentTypes;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ContentTypeListExtensions
{
    /// <param name="list"></param>
    extension(IEnumerable<IContentType> list)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="includeAttributeTypes">Include (or filter out) special types starting with "@"</param>
        /// <returns></returns>
        [ShowApiWhenReleased(ShowApiMode.Never)]
        public IEnumerable<IContentType> OfScope(string? scope = null, bool includeAttributeTypes = false)
        {
            var set = list
                .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));

            // New v20.00-09: Support for wildcard to get all scopes
            if (!string.IsNullOrWhiteSpace(scope) && scope != "*")
                set = set.Where(p => p.Scope == scope);

            return set.OrderBy(c => c.Name);
        }

        private IList<string> GetAllScopesInclDefault()
        {
            var scopes = list
                .Select(ct => ct.Scope)
                .Distinct()
                .ToListOpt();

            // Make sure the "Default" scope is always included, otherwise it's missing on new apps
            if (!scopes.Contains(ScopeConstants.Default))
                scopes = scopes
                    .Append(ScopeConstants.Default)
                    .ToListOpt();

            // Add new Configuration scope for v12.02
            if (!scopes.Contains(ScopeConstants.SystemConfiguration))
                scopes = scopes
                    .Append(ScopeConstants.SystemConfiguration)
                    .ToListOpt();

            return scopes
                .OrderBy(s => s)
                .ToListOpt();
        }

        [ShowApiWhenReleased(ShowApiMode.Never)]
        public IDictionary<string, string> GetAllScopesWithLabels()
        {
            var scopes = list.GetAllScopesInclDefault();
            var lookup = ScopeConstants.ScopesWithNames;
            var results = scopes
                .Select(s => new { value = s, name = lookup.TryGetValue(s, out var label) ? label : s })
                .OrderByDescending(s => s.name == ScopeConstants.Default)
                .ThenBy(s => s.name)
                .ToDictionary(s => s.value, s => s.name);
            return results;
        }
    }
}