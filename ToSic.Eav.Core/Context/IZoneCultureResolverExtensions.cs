using System.Globalization;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;

namespace ToSic.Eav.Context
{
    // ReSharper disable once InconsistentNaming
    [PrivateApi("this is all very internal, and names may still change")]
    public static class IZoneCultureResolverExtensions
    {
        public static string SafeCurrentCultureCode(this IZoneCultureResolver resolver)
            => resolver != null
                ? resolver.CurrentCultureCode.ToLowerInvariant()
                : ThreadCurrentCultureInfo.Name;

        public static string[] SafeCurrentDimensions(this IZoneCultureResolver resolver)
            => new[] {SafeCurrentCultureCode(resolver)};

        public static CultureInfo SafeCurrentCultureInfo(string[] dimensions)
        {
            try
            {
                var dimension = dimensions?.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(dimension))
                    return CultureInfo.GetCultureInfo(dimension);
            }
            catch { /* ignore */}

            return ThreadCurrentCultureInfo;
        }

        private static CultureInfo ThreadCurrentCultureInfo => CultureInfo.CurrentCulture;

        public static CultureInfo SafeCurrentCultureInfo(this IZoneCultureResolver resolver) =>
            resolver == null 
                ? ThreadCurrentCultureInfo 
                : CultureInfo.GetCultureInfo(resolver.SafeCurrentCultureCode());

        /// <summary>
        /// This shouldn't be used because it's thread based, but there are some cases on the
        /// Entity which will need this. We're setting all calls to this
        /// so we can keep track of where they are used
        /// </summary>
        /// <returns></returns>
        public static string ThreadCultureNameNotGood() => ThreadCurrentCultureInfo.Name;
    }
}
