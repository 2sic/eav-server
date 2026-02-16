using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace ToSic.Sys.Utils;

/// <summary>
/// Special helper to create objects from a type.
/// </summary>
/// <remarks>
/// The type must have a parameterless constructor, otherwise it will throw an error.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class TypeFactory
{
    /// <summary>
    /// A thread-safe cache to store our compiled delegates.
    /// These are important for fast creation without ongoing reflection.
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, Func<object>> Cache = new();

    /// <summary>
    /// The Generic Wrapper: Uses the central cache but returns the correct type.
    /// </summary>
    public static T CreateInstance<T>() where T: class =>
        (T)CreateInstanceFunc(typeof(T))();

    public static object CreateInstance(Type type) =>
        CreateInstanceFunc(type)();

    /// <summary>
    /// GetOrAdd ensures we only compile the expression once
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Func<object> CreateInstanceFunc(Type type) =>
        Cache.GetOrAdd(type, t =>
        {
            // If it's a value type (struct), we use Activator or a specialized expression
            // but for simplicity, here is the standard class implementation:
            if (t.IsValueType)
                return () => Activator.CreateInstance(t)!;

            var constructor = t.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new InvalidOperationException($"Type {t.Name} must have a public parameterless constructor.");

            // Generate the IL equivalent of: () => new T()
            var newExp = Expression.New(t);
            var lambda = Expression.Lambda<Func<object>>(newExp);
            return lambda.Compile();
        });
}