using System.Reflection;
using ToSic.Eav.Data;

namespace ToSic.Eav.Models;

public class ToModelTacInternal : IToModelTac
{
    public TModel? ToModelTac<TModel>(IEntity? entity, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => ToModelExtensions.ToModelInternal<TModel>(entity, options ?? new());

    public object? ToModel(IEntity entity, Type type, string? typeName)
        => ToModelInternalTac(entity, type, options: new() { TypeName = typeName });
    
    /// <summary>
    /// Special variant with type as parameter, using reflection
    /// </summary>
    private static object? ToModelInternalTac(IEntity? entity, Type type, ToModelOptions options)
    {
        // As we invoke it, we must be sure to return the inner exception
        try
        {
            // Invoke the method. The arguments are: entity, npo, options
            return ToModelInternalMethodInfo.MakeGenericMethod(type)
                .Invoke(null, [entity, options, null, nameof(ToModelInternalTac)]);
        }
        catch (TargetInvocationException ex)
        {
            // Rethrow the inner exception to preserve the original stack trace
            throw ex.InnerException ?? ex;
        }
    }

    private static MethodInfo ToModelInternalMethodInfo => field
        ??= typeof(ToModelExtensions)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(m => m is
                {
                    Name: nameof(ToModelExtensions.ToModelInternal),
                    IsGenericMethod: true
                })
            ?? throw new InvalidOperationException($"Method '{nameof(ToModelExtensions.ToModelInternal)}' not found or not generic.");
}