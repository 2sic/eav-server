namespace ToSic.Eav.Plumbing.ObjectExtension;

internal static class TestAccessors
{
    /// <summary>
    /// Test Accessor to prevent too many use counts on the real method
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="argument"></param>
    /// <param name="treatFalseAsDefault"></param>
    /// <returns></returns>
    public static bool IsNullOrDefaultTac<TObject>(this TObject argument, bool treatFalseAsDefault = true) 
        => argument.IsNullOrDefault(treatFalseAsDefault);


    public static T ConvertOrDefaultTac<T>(this object found, bool numeric = false, bool truthy = false)
        => found.ConvertOrDefault<T>(numeric: numeric, truthy: truthy);

    public static T ConvertOrFallbackTac<T>(this object value, T fallback, bool numeric = false, bool truthy = false, bool fallbackOnDefault = false)
        => value.ConvertOrFallback(fallback, numeric, truthy, fallbackOnDefault);

}