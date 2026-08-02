using System.Reflection;
using ToSic.Eav.Data;
using ToSic.Eav.Models.Factory;
using ToSic.Sys.Coding;

namespace ToSic.Eav.Models;

public static class ToModelTestAccessors
{
    #region ToModelInternal

    internal static TModel? ToModelInternalTac<TModel>(
        this IEntity? entity,
        ToModelOptions options,
        NoParamOrder npo = default
    )
        where TModel : class, IModelFromEntity
        => entity.ToModelInternal<TModel>(options, npo);

    internal static object? ToModelInternalTac(
        this IEntity? entity,
        Type type,
        ToModelOptions options,
        NoParamOrder npo = default
    )
    {
        // We need to dynamically find the correct generic method to call
        // It's named ToModel and is defined in ToModelExtensions
        // We need to find the one with IsGenericMethod = true
        var methods = typeof(ToModelIntern).GetMethods(BindingFlags.Static | BindingFlags.Public);
        var genericMethod =
            methods.FirstOrDefault(m => m is { Name: nameof(ToModelIntern.ToModelInternal), IsGenericMethod: true });

        if (genericMethod == null)
            throw new InvalidOperationException(
                $"Method '{nameof(ToModelIntern.ToModelInternal)}' not found or not generic.");

        // Make the generic method specific to the target type 'type'
        var specificGenericMethod = genericMethod.MakeGenericMethod(type);

        // Invoke the method. The arguments are: entity, npo, options
        // We are passing 'entity' and 'options' as they are.
        // 'npo' is the default NoParamOrder.
        var result = specificGenericMethod.Invoke(null, [entity, options, npo, null, nameof(ToModelInternalTac)]);

        // The result of Invoke is object?, so we return it as is.
        return result;
        
        //return entity.ToModelInternal<TModel>(options, npo);
    }

    #endregion

    #region ToModel (Single Entity / CanBeEntity)

    internal static TModel? ToModelTac<TModel>(this IEntity? entity)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>();

    internal static TModel? ToModelTac<TModel>(
        this IEntity? entity,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(npo, options: options);

    /// <summary>
    /// Non-generic test call to handle cases where we have the type but not as a generic parameter.
    /// This is useful for testing and dynamic scenarios.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="type"></param>
    /// <param name="npo"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    internal static object? ToModelTac(
        this IEntity? entity,
        Type type,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
    {
        // As we invoke it, we must be sure to return the inner exception
        try
        {
            // Make the generic method specific to the target type
            var specificGenericMethod = ToModelMethodInfo.MakeGenericMethod(type);

            // Invoke the method with the provided entity, options, and other parameters
            var result = specificGenericMethod.Invoke(null, [entity, npo, options]);

            // The result of Invoke is object?, so we need to cast it to the expected type.
            // Since the `type` parameter is a `Type`, we can cast it like this:
            return result;
        }
        catch (TargetInvocationException ex)
        {
            // Rethrow the inner exception to preserve the original stack trace
            throw ex.InnerException ?? ex;
        }
    }

    // Use reflection to call the generic ToModel method with the specified type
    private static MethodInfo ToModelMethodInfo => field
        ??= typeof(ToModelExtensions)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m is
                                     {
                                         Name: nameof(ToModelExtensions.ToModel),
                                         IsGenericMethod: true
                                     }
                                     && m.GetParameters().Length == 3
                )
            ?? throw new InvalidOperationException(
                $"Could not find the generic method '{nameof(ToModelExtensions.ToModel)}'.");

    /// <summary>
    /// ICanBeEntity Overload
    /// </summary>
    public static TModel? ToModelTac<TModel>(
        this ICanBeEntity? canBeEntity,
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity
    => canBeEntity.ToModel<TModel>(npo, options: options);

    #endregion


    #region ToModel with Factory (Single Entity / CanBeEntity TODO:)

    public static TModel? ToModelTac<TModel>(this IEntity entity, IModelFactory factory)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(factory);


    #endregion


    internal static TModel? FirstModelTac<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>(npo, options);

    public static TModel? FirstModelTac<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>();

}