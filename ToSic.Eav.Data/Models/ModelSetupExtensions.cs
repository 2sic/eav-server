namespace ToSic.Eav.Models;

// Must keep private for now, as it somehow ends up on every object in the docs
[PrivateApi]
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

    ///// <summary>
    ///// Helper to set up the data being wrapped, returning the wrapper for easy chaining.
    ///// </summary>
    //[return: NotNullIfNotNull(nameof(data))]
    //internal static TModel? CreateAndSetup<TModel, TData>(this TModel model, TData? data,
    //    NullToModel nullHandling)
    //    where TData : class
    //    where TModel : class, IModelSetup<TData>, new()
    //{
    //    if (data == null)
    //    {
    //        if ((nullHandling & NullToModel.DataAsNull) != 0)
    //            return default;
    //        if ((nullHandling & NullToModel.DataAsThrow) != 0)
    //            throw new InvalidCastException("data is null");
    //    }

    //}

    /// <summary>
    /// Helper to set up the data being wrapped, returning the wrapper for easy chaining.
    /// </summary>
    [return: NotNullIfNotNull(nameof(data))]
    internal static TModel? SetupWithDataNullChecks<TModel, TData>(this TModel model, TData? data, NullToModel nullHandling)
        where TData : class
        where TModel : IModelSetup<TData>
    {
        if (data == null)
        {
            if ((nullHandling & NullToModel.DataAsNull) != 0)
                return default;
            if ((nullHandling & NullToModel.DataAsThrow) != 0)
                throw new InvalidCastException("data is null");
        }

        var ok = model.SetupModel(data);
        return ok
            ? model
            : (nullHandling & NullToModel.DataAsModelForce) != 0
                ? model
                : (nullHandling & NullToModel.DataAsModelOrThrow) != 0
                    ? throw new InvalidCastException("data is null")
                    : default;
    }
}