using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

/// <summary>
/// Dependency Injection helper to override a service type with a different implementation.
/// </summary>
/// <remarks>
/// When `using (...)` this, it will ensure that any code within the `using` will receive the overridden implementation, while code outside will receive the original implementation.
///
/// This will only have an effect, if a service was initially registered for overriding, using the various <see cref="Register{T}" /> methods.
/// </remarks>
/// <typeparam name="TService"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static partial class OverrideService<TService> where TService : class
{
    /// <summary>
    /// Special container to hold the replaced factory function for the current async context.
    /// </summary>
    private static readonly AsyncLocal<Func<IServiceProvider, TService>?> Factory = new();

    public static Func<IServiceProvider, TService>? CurrentFactory => Factory.Value;

    /// <summary>
    /// Use Overload 1: Supply a type (preserves transient lifetime via DI container)
    /// </summary>
    /// <typeparam name="TImplementation">The type to resolve instead, must be registered with the DI container.</typeparam>
    public static IDisposable Use<TImplementation>()
        where TImplementation : class, TService
        => Use(sp => sp.GetRequiredService<TImplementation>());

    /// <summary>
    /// Use Overload 2: Supply a specific instance (singleton)
    /// </summary>
    /// <typeparam name="TImplementation">The type of the instance to use.</typeparam>
    /// <param name="value">The specific instance to use.</param>
    public static IDisposable Use<TImplementation>(TImplementation value)
        where TImplementation : class, TService
        => Use(_ => value);

    /// <summary>
    /// Use Overload 3: Supply a custom factory function
    /// </summary>
    /// <param name="factory">The factory function to use.</param>
    public static IDisposable Use(Func<IServiceProvider, TService> factory)
    {
        // Create a scope for the `using`, but remember the previous value so we can restore it when the scope is disposed.
        var newScope = new Scope(Factory.Value);

        // Set the new factory for the current async context
        Factory.Value = factory;

        // Return the scope, which will restore the previous factory when disposed
        return newScope;
    }

    /// <summary>
    /// Tiny helper class to hold the generator.
    /// </summary>
    /// <param name="previousValue">Previous generator (if there was one)</param>
    private class Scope(Func<IServiceProvider, TService>? previousValue) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            Factory.Value = previousValue; // Restores outer override or null
            _disposed = true;
        }
    }
}