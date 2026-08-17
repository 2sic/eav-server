using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI.ChildScope;


public static class OverrideContext<TService> where TService : class
{
    private static readonly AsyncLocal<Func<IServiceProvider, TService>?> Factory = new();

    public static Func<IServiceProvider, TService>? CurrentFactory => Factory.Value;

    // Overload 1: Supply a type (preserves transient lifetime via DI container)
    public static IDisposable Begin<TImplementation>()
        where TImplementation : class, TService
        => Begin(sp => sp.GetRequiredService<TImplementation>());

    public static IDisposable Begin<TImplementation>(TImplementation value)
        where TImplementation : class, TService
        => Begin(_ => value);

    // Overload 2: Supply a custom factory function
    public static IDisposable Begin(Func<IServiceProvider, TService> factory)
    {
        var newScope = new Scope(Factory.Value);
        Factory.Value = factory;
        return newScope;
    }

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


    /// <summary>
    /// Generate the factory function to either get from the override, or from the underlying service provider.
    /// </summary>
    /// <typeparam name="TService">Service type or interface</typeparam>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <returns>A factory function to create the service instance</returns>
    public static Func<IServiceProvider, TService> Register<TImplementation>()
        where TImplementation : class, TService
    {
        // Check if an override is active in the current async context
        return sp => CurrentFactory is { } overrideFactory
            // 1. Return from new factory
            ? overrideFactory(sp)
            // 2. Fall back to the default implementation in the SAME scope
            : sp.GetRequiredService<TImplementation>();
    }
    
    /// <summary>
    /// Generate the factory function to either get from the override, or from the underlying service provider.
    /// </summary>
    /// <typeparam name="TService">Service type or interface</typeparam>
    /// <returns>A factory function to create the service instance</returns>
    public static Func<IServiceProvider, TService> Register()
        => Register<TService>();
}