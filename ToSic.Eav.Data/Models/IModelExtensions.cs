namespace ToSic.Eav.Models;

public static class IModelExtensions
{
    /// <summary>
    /// Helper to set up the data being wrapped, returning the wrapper for easy chaining.
    /// </summary>
    [PrivateApi]
    [return: NotNullIfNotNull(nameof(source))]
    public static TWrapper? Setup<TWrapper, TSource>(this TWrapper wrapper, TSource? source)
        where TSource : class
        where TWrapper : IModelSetup<TSource>
    {
        var ok = wrapper.SetupModel(source);
        return ok ? wrapper : default;
    }
}