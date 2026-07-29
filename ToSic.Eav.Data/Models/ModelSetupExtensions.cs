using static ToSic.Eav.Models.ToModelOptions;

namespace ToSic.Eav.Models;

// Must keep private for now, as it somehow ends up on every object in the docs
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ModelSetupExtensions
{
    /// <summary>
    /// Helper to set up the data being wrapped, returning the wrapper for easy chaining.
    /// </summary>
    [return: NotNullIfNotNull(nameof(source))]
    public static TWrapper? Setup<TWrapper, TSource>(this TWrapper wrapper, TSource? source)
        where TSource : class
        where TWrapper : IModelSetup<TSource>
    {
        var ok = wrapper.SetupModel(source);
        return ok ? wrapper : default;
    }

    /// <summary>
    /// Helper to set up the data being wrapped, returning the wrapper for easy chaining.
    /// </summary>
    [return: NotNullIfNotNull(nameof(data))]
    internal static TModel? SetupWithDataNullChecks<TModel, TData>(this TModel model, TData? data, DataNullHandling nullHandling)
        where TData : class
        where TModel : IModelSetup<TData>
    {
        if (data == null)
        {
            switch (nullHandling)
            {
                case DataNullHandling.Undefined or DataNullHandling.AsNull:
                    return default;
                case DataNullHandling.Throw:
                    throw new InvalidCastException("data is null");
            }
        }

        var ok = model.SetupModel(data);
        return ok
            ? model
            : nullHandling == DataNullHandling.ConvertForce
                ? model
                : nullHandling == DataNullHandling.ConvertOrThrow
                    ? throw new InvalidCastException("data is null")
                    : default;
    }
}