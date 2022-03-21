using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data.PiggyBack
{
    [PrivateApi]
    // ReSharper disable once InconsistentNaming
    public static class IHasPiggyBackExtensions
    {

        [PrivateApi]
        public static TData GetPiggyBack<TData>(this IHasPiggyBack parent, string key, Func<TData> create) 
            => parent.PiggyBack.GetOrGenerate(key, create);

        /// <summary>
        /// Use a property lookup to get a value, and if it's from a piggy-back source, use the pre-made rich object.
        /// Otherwise create and add to piggyback.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="entryPoint"></param>
        /// <param name="field"></param>
        /// <param name="factory"></param>
        /// <param name="logOrNull"></param>
        /// <returns></returns>
        [PrivateApi]
        public static TData GetOrCreateInPiggyBack<TData>(this IPropertyLookup entryPoint, string field, Func<string, TData> factory, ILog logOrNull) where TData : class
        {
            var wrapLog = logOrNull.SafeCall<TData>();
            var advProperty = entryPoint.FindPropertyInternal(field, Array.Empty<string>(), logOrNull);

            // Skip if nothing to process
            if (!(advProperty?.Result is string valString) || string.IsNullOrWhiteSpace(valString))
                return wrapLog("empty / not found", null);

            // If our source has a PiggyBack cache, use this
            if (advProperty.Source is IHasPiggyBack piggyBackCache)
                return wrapLog("piggyback", piggyBackCache.GetPiggyBack("auto-pgb-" + field, () => factory(valString)));

            // Otherwise just create
            return wrapLog("no piggyback", factory(valString));
        }


    }
}
