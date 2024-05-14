namespace ToSic.Eav.Plumbing;

static partial class ObjectExtensions
{
    // TODO: SEARCH FOR "== null ? null" and replace with this helper

    /// <summary>
    /// If obj is null, return null, otherwise return the result of func(obj)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="obj"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static TResult NullOrGetWith<T, TResult>(this T obj, Func<T, TResult> func)
        where T : class
        where TResult : class
        => obj == null ? null : func(obj);

    public static TResult Map<T, TResult>(this T obj, Func<T, TResult> func) where TResult : class
        => func(obj);


    /// <summary>
    /// If obj is null, return null, otherwise return the result of func(obj)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="obj"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static TResult NullOrGet<T, TResult>(this T obj, Func<TResult> func)
        where T : class
        where TResult : class
        => obj == null ? null : func();


    public static void DoIfNotNull<T>(this T obj, Action<T> func)
        where T : class
    {
        if (obj == null) return;
        func(obj);
    }

    public static void DoIfNotNull<T>(this T obj, Action func)
        where T : class
    {
        if (obj == null) return;
        func();
    }
}