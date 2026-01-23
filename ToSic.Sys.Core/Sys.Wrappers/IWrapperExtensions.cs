namespace ToSic.Sys.Wrappers;

public static class IWrapperExtensions
{
    /// <summary>
    /// Helper to set up the data being wrapped, returning the wrapper for easy chaining.
    /// </summary>
    [PrivateApi]
    [return: NotNullIfNotNull(nameof(source))]
    public static TWrapper? Setup<TWrapper, TSource>(this TWrapper wrapper, TSource? source)
        where TSource : class
        where TWrapper : IWrapperSetup<TSource>
    {
        var ok = wrapper.SetupContents(source);
        return ok ? wrapper : default;
    }
}