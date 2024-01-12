using System;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data.PiggyBack;

[PrivateApi]
// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class IHasPiggyBackExtensions
{

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static TData GetOrCreateInPiggyBack<TData>(this IPropertyLookup entryPoint, string field, Func<string, TData> factory, ILog logOrNull) where TData : class
    {
        var wrapLog = logOrNull.Fn<TData>();
        var advProperty = entryPoint.FindPropertyInternal(new(field), null);

        // Skip if nothing to process
        if (!(advProperty?.Result is string valString) || string.IsNullOrWhiteSpace(valString))
            return wrapLog.ReturnNull("empty / not found");

        // If our source has a PiggyBack cache, use this
        if (advProperty.Source is IHasPiggyBack piggyBackCache)
            return wrapLog.Return(piggyBackCache.GetPiggyBack("auto-pgb-" + field, () => factory(valString)), "piggyback");

        // Otherwise just create
        return wrapLog.Return(factory(valString), "no piggyback");
    }


}