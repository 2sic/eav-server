using System.Runtime.CompilerServices;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Sys.PropertyLookup;
using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Apps.Sys;

[PrivateApi]
// ReSharper disable once InconsistentNaming
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class IHasPiggyBackExtensions
{
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
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TData? GetOrCreateInPiggyBack<TData>(this IPropertyLookup entryPoint, string field, Func<string, TData> factory, ILog logOrNull) where TData : class
    {
        var l = logOrNull.Fn<TData>();
        var advProperty = entryPoint.FindPropertyInternal(new(field), new());

        // Skip if nothing to process
        if (advProperty?.Result is not string valString || string.IsNullOrWhiteSpace(valString))
            return l.ReturnNull("empty / not found");

        // If our source has a PiggyBack cache, use this
        if (advProperty.Source is IHasPiggyBack piggyBackCache)
            return l.Return(piggyBackCache.PiggyBackGet("auto-pgb-" + field, () => factory(valString)), "piggyback");

        // Otherwise just create
        return l.Return(factory(valString), "no piggyback");
    }
}