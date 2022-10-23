using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Context
{
    // ReSharper disable once InconsistentNaming
    [PrivateApi("this is all very internal, and names may still change")]
    public static class IZoneCultureResolverExtensions
    {
        public static string SafeCurrentCultureCode(this IZoneCultureResolver resolver)
            => (resolver?.CurrentCultureCode ?? ThreadCurrentCultureInfo.Name).ToLowerInvariant();

        public static CultureInfo SafeCultureInfo(string[] dimensions)
        {
            try
            {
                if (dimensions == null || dimensions.Length == 0) return ThreadCurrentCultureInfo;
                var d = dimensions.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(d)) return CultureInfo.GetCultureInfo(d);
            }
            catch { /* ignore */}

            return ThreadCurrentCultureInfo;
        }

        private static CultureInfo ThreadCurrentCultureInfo => CultureInfo.CurrentCulture;

        public static CultureInfo SafeCurrentCultureInfo(this IZoneCultureResolver resolver) =>
            resolver == null 
                ? ThreadCurrentCultureInfo 
                : CultureInfo.GetCultureInfo(resolver.SafeCurrentCultureCode());



        #region Language Priorities list

        public static string[] SafeLanguagePriorityCodes(this IZoneCultureResolver resolver)
        {
            if (resolver == null) return new[] {SafeCurrentCultureCode(null), null};

            var priorities = new List<string>();
            // First priority: current culture
            if (!string.IsNullOrWhiteSpace(resolver.CurrentCultureCode)) 
                priorities.Add(SafeCurrentCultureCode(resolver));
            // Second priority: Fallback culture
            if (!string.IsNullOrWhiteSpace(resolver.DefaultCultureCode))
                priorities.Add(resolver.DefaultCultureCode.ToLowerInvariant());
            // last priority: the null-entry, meaning that it should just pick anything
            priorities.Add(null);

            return priorities.ToArray();
        }

        #endregion
    }
}
