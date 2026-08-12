using System.Runtime.CompilerServices;

namespace ToSic.Sys.Caching.PiggyBack;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class IHasPiggyBackExtensions
{
    /// <summary>
    /// Get a piggybacked value from the parent, or create it if it doesn't exist.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="parent"></param>
    /// <param name="key"></param>
    /// <param name="create"></param>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public static TData PiggyBackGet<TData>(this IHasPiggyBack parent, string key, Func<TData> create) 
    {
        var cache = parent.PiggyBack;
        if (cache.TryGetValue(key, out var result) && result is TData typed)
            return typed;

        typed = create();

        try { cache.TryAdd(key, typed); }
        catch { /* ignore */ }
        return typed;
    }

    /// <summary>
    /// Remove a piggybacked value from the parent.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="key"></param>
    public static void PiggyBackRemove(this IHasPiggyBack parent, string key)
        => _ = parent.PiggyBack.TryRemove(key, out _);


    /// <summary>
    /// Get a value from the Piggyback or create it.
    /// Will expire if the parent is updated.
    /// </summary>
    /// <remarks>
    /// Use this to piggy-back data which may need to expire if the parent changes.
    /// If the parent is timestamped
    /// </remarks>
    /// <typeparam name="TData"></typeparam>
    /// <param name="parent"></param>
    /// <param name="expiring">Parent object with timestamp.</param>
    /// <param name="key">Key to use in the caching dictionary, should be unique for the expected use case.</param>
    /// <param name="create">Generator function</param>
    /// <returns></returns>
    public static (TData Value, bool IsCached) PiggyBackGetExpiring<TData>(this IHasPiggyBack parent, ITimestamped expiring, string key, Func<TData> create)
    {
        // Check if exists and timestamp still ok, return that
        var cache = parent.PiggyBack;
        if (cache.TryGetValue(key, out var result)
            && result is Timestamped<TData> typed
            && typed.CacheTimestamp == expiring.CacheTimestamp
           )
            return (typed.Value, true);

        // else create it, add timestamp, and store
        try
        {
            var newValue = create();
            var timestamped = new Timestamped<TData>(newValue, expiring.CacheTimestamp);
            cache.TryAdd(key, timestamped);
            return (timestamped.Value, false);
        }
        catch { /* ignore / silent */ }

        return default;
    }

    /// <summary>
    /// Get from piggyback, while using the parent as the cache/expiring parameter to ensure reload when app changes
    /// </summary>
    /// <param name="parent">Parent with Piggy-Back and timestamp information</param>
    /// <param name="key"></param>
    /// <param name="create"></param>
    /// <returns></returns>
    public static (TData Value, bool IsCached) PiggyBackGetExpiring<TPiggyBack, TData>(
        this TPiggyBack parent,
        string key,
        Func<TData> create
    )
        where TPiggyBack : IHasPiggyBack, ITimestamped
        => parent.PiggyBackGetExpiring(parent, key, create);

    /// <summary>
    /// Get PiggyBack property using automatic name.
    /// Will auto-expire if the app has any changes on it.
    /// Uses the caller object name and method for the key.
    /// </summary>
    /// <param name="parent">Parent with Piggy-Back and timestamp information</param>
    /// <param name="create"></param>
    /// <param name="cPath">auto</param>
    /// <param name="cName">auto</param>
    /// <returns></returns>
    public static (TData Value, bool IsCached) PiggyBackGetExpiring<TPiggyBack, TData>(
        this TPiggyBack parent,
        Func<TData> create,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default
    )
        where TPiggyBack : IHasPiggyBack, ITimestamped
        => parent.PiggyBackGetExpiring(parent, $"autokey:{cPath};{cName}()", create);
}