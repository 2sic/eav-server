namespace ToSic.Eav.Data.ContentTypes.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ContentTypeListExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEnumerable<IContentType> OfScope(this IEnumerable<IContentType> list, string scope = null, bool includeAttributeTypes = false)
    {
        var set = list
            .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
        
        if (scope != null)
            set = set.Where(p => p.Scope == scope);

        return set.OrderBy(c => c.Name);
    }

    private static IList<string> GetAllScopesInclDefault(this IEnumerable<IContentType> list)
    {
        var scopes = list
            .Select(ct => ct.Scope)
            .Distinct()
            .ToList();

        // Make sure the "Default" scope is always included, otherwise it's missing on new apps
        if (!scopes.Contains(ScopeConstants.Default))
            scopes.Add(ScopeConstants.Default);

        // Add new Configuration scope for v12.02
        if (!scopes.Contains(ScopeConstants.SystemConfiguration))
            scopes.Add(ScopeConstants.SystemConfiguration);

        return scopes
            .OrderBy(s => s)
            .ToArray();
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IDictionary<string, string> GetAllScopesWithLabels(this IEnumerable<IContentType> list)
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