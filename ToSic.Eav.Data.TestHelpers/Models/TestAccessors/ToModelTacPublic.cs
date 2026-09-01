using System.Reflection;
using ToSic.Eav.Data;

namespace ToSic.Eav.Models;

public class ToModelTacPublic : IToModelTac
{
    public TModel? ToModelTac<TModel>(IEntity? entity, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(options: options);

    public object? ToModel(IEntity entity, Type type, string? typeName)
        => ToModelTac(entity, type, options: new() { TypeName = typeName });

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
        IEntity? entity,
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

}