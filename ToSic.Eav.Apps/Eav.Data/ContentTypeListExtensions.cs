﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Data
{
    public static class ContentTypeListExtensions
    {
        public static IEnumerable<IContentType> OfScope(this IEnumerable<IContentType> all, string scope = null, bool includeAttributeTypes = false)
        {
            var set = all.Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
            if (scope != null)
                set = set.Where(p => p.Scope == scope);
            return set.OrderBy(c => c.Name);
        }

        public static IList<string> GetAllScopesInclDefault(this IEnumerable<IContentType> all)
        {
            var scopes = all.Select(ct => ct.Scope).Distinct().ToList();

            // Make sure the "Default" scope is always included, otherwise it's missing on new apps
            if (!scopes.Contains(AppConstants.ScopeContentOld))
                scopes.Add(AppConstants.ScopeContentOld);
            
            // Add new Configuration scope for v12.02
            if(!scopes.Contains(AppConstants.ScopeConfiguration))
                scopes.Add(AppConstants.ScopeConfiguration);

            return scopes.OrderBy(s => s).ToArray();
        }

        public static IDictionary<string, string> GetAllScopesWithLabels(this IEnumerable<IContentType> all)
        {
            var scopes = all.GetAllScopesInclDefault();

            var lookup = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {AppConstants.ScopeContentOld, "Default"},
                {AppConstants.ScopeConfiguration, "Configuration (Views etc.)" },
                {AppConstants.ScopeContentSystem, "System: CMS"},
                {AppConstants.ScopeApp, "System: App"},
                {Constants.ScopeSystem, "System: System"},
                {"System.DataSources", "System: DataSources"},
                {"System.Decorators", "System: Decorators"},
                {"System.Fields", "System: Fields"}
            };

            var results = scopes
                .Select(s => new { value = s, name = lookup.TryGetValue(s, out var label) ? label : s })
                .OrderBy(s => s.name)
                .ToDictionary(s => s.value, s => s.name);
            return results;
        }

    }
}
