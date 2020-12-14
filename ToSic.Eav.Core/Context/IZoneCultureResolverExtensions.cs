using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

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
            // No resolver? just assume fallback only
            if (resolver == null) return new[] {SafeCurrentCultureCode(null), null};

            return GenerateLanguagePriorities(resolver);// resolver.LanguagePriorityCodes;
        }

        public static string[] GenerateLanguagePriorities(this IZoneCultureResolver resolver)
        {
            var wrapLog = (resolver as IHasLog)?.Log.Call();

            var priorities = new List<string>();
            // First priority: current culture
            if (!string.IsNullOrWhiteSpace(resolver?.CurrentCultureCode)) 
                priorities.Add(SafeCurrentCultureCode(resolver));
            // Second priority: Fallback culture
            if (!string.IsNullOrWhiteSpace(resolver?.DefaultCultureCode))
                priorities.Add(resolver.DefaultCultureCode.ToLowerInvariant());
            // last priority: the null-entry, meaning that it should just pick anything
            priorities.Add(null);

            var priorityArray = priorities.ToArray();
            wrapLog?.Invoke(string.Join(",", priorityArray));
            return priorityArray;
        }

        #endregion



        /// <summary>
        /// This shouldn't be used because it's thread based, but there are some cases on the
        /// Entity which will need this. We're setting all calls to this
        /// so we can keep track of where they are used
        /// </summary>
        /// <returns></returns>
        public static string ThreadCultureNameNotGood() => ThreadCurrentCultureInfo.Name;
    }
}
