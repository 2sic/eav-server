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
    internal static TModel? SetupWithDataNullChecks<TModel, TData>(this TModel model, TData? data, NullHandling nullHandling)
        where TData : class
        where TModel : IModelSetup<TData>
    {
        return data switch
        {
            // data Null with throw
            null when nullHandling == NullHandling.Throw
                => throw new InvalidCastException("data is null"),

            // data Null with default / AsNull
            null when nullHandling is NullHandling.Default or NullHandling.ReturnNull
                => default,

            // data Null with other or non-null
            // Try to set up the model, and get feedback if it seems ok.
            // then return based on ok status and conversion options.
            _ => model.SetupModel(data) switch
            {
                true => model,
                false => nullHandling switch
                {
                    NullHandling.ReturnModel => model,
                    NullHandling.TryOrNull => default,
                    NullHandling.TryOrThrow => throw new InvalidCastException("data is null"),
                    _ => default
                }
            }
        };
    }
}