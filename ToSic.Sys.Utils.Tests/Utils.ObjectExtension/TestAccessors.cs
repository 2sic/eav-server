namespace ToSic.Sys.Utils.ObjectExtension;

internal static class TestAccessors
{
    public static (bool Success, T? Value) TryConvertTac<T>(this object? value, bool numeric = false, bool truthy = false)
        => value.TryConvert<T>(numeric: numeric, truthy: truthy);

    /// <summary>
    /// Test Accessor to prevent too many use counts on the real method
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="value"></param>
    /// <param name="treatFalseAsDefault"></param>
    /// <returns></returns>
    public static bool IsNullOrDefaultTac<TObject>(this TObject value, bool treatFalseAsDefault = true) 
        => value.IsNullOrDefault(treatFalseAsDefault);


    public static T? ConvertOrDefaultTac<T>(this object? value, bool numeric = false, bool truthy = false)
        => value.ConvertOrDefault<T>(numeric: numeric, truthy: truthy);

    public static T ConvertOrFallbackTac<T>(this object? value, T fallback, bool numeric = false, bool truthy = false, bool fallbackOnDefault = false)
        => value.ConvertOrFallback(fallback, numeric, truthy, fallbackOnDefault);

}