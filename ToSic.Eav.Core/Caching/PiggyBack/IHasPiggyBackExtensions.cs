using System.Runtime.CompilerServices;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Caching.PiggyBack;

namespace ToSic.Eav.Data.PiggyBack;

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
    public static TData GetOrCreateInPiggyBack<TData>(this IPropertyLookup entryPoint, string field, Func<string, TData> factory, ILog logOrNull) where TData : class
    {
        var l = logOrNull.Fn<TData>();
        var advProperty = entryPoint.FindPropertyInternal(new(field), null);

        // Skip if nothing to process
        if (advProperty?.Result is not string valString || string.IsNullOrWhiteSpace(valString))
            return l.ReturnNull("empty / not found");

        // If our source has a PiggyBack cache, use this
        if (advProperty.Source is IHasPiggyBack piggyBackCache)
            return l.Return(piggyBackCache.GetPiggyBack("auto-pgb-" + field, () => factory(valString)), "piggyback");

        // Otherwise just create
        return l.Return(factory(valString), "no piggyback");
    }

    /// <summary>
    /// Get from piggyback, while using the AppState itself as the cache/expiring parameter to ensure reload when app changes
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="parent"></param>
    /// <param name="key"></param>
    /// <param name="create"></param>
    /// <returns></returns>
    public static (TData Value, bool IsCached) GetPiggyBackExpiring<TData>(this IAppReader parent, string key, Func<TData> create)
    {
        var appState = parent.GetCache();
        return appState.PiggyBack.GetOrGenerate(appState, key, create);
    }


    /// <summary>
    /// Get PiggyBack property, and if it doesn't exist, create it.
    /// Will auto-expire if the app has any changes on it.
    /// Uses the file name and method for the key.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="parent"></param>
    /// <param name="create"></param>
    /// <param name="cPath">auto</param>
    /// <param name="cName">auto</param>
    /// <returns></returns>
    public static (TData Value, bool IsCached) GetPiggyBackPropExpiring<TData>(this IAppReader parent, Func<TData> create,
        [CallerFilePath] string cPath = default, [CallerMemberName] string cName = default)
    {
        var appState = parent.GetCache();
        return appState.PiggyBack.GetOrGenerate(appState, $"autokey:{cPath};{cName}()", create);
    }
}