using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

public static partial class OverrideService<TService>
{

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
    /// Register as if it were scoped.
    /// We need this, because the true registration must be transient (otherwise the factory would never be rechecked).
    /// But we still want to be able to have the initial registration behave as scoped, which is why we do this.
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    public static Func<IServiceProvider, TService> RegisterScoped<TImplementation>()
        where TImplementation : class, TService
    {
        // Check if an override is active in the current async context
        TService? cache = null;
        return sp => CurrentFactory is { } overrideFactory
            // 1. Return from new factory
            ? overrideFactory(sp)
            // 2. Fall back to the default implementation in the SAME scope
            : cache ??= sp.GetRequiredService<TImplementation>();
    }
    
    /// <summary>
    /// Generate the factory function to either get from the override, or from the underlying service provider.
    /// </summary>
    /// <typeparam name="TService">Service type or interface</typeparam>
    /// <returns>A factory function to create the service instance</returns>
    public static Func<IServiceProvider, TService> Register()
        => Register<TService>();

    public static Func<IServiceProvider, TService> RegisterScoped()
        => RegisterScoped<TService>();

    /// <summary>
    /// Register with a custom factory method
    /// </summary>
    /// <param name="factory">A factory function to create the service instance</param>
    /// <returns>A factory function to create the service instance</returns>
    public static Func<IServiceProvider, TService> Register(Func<TService> factory)
    {
        // Check if an override is active in the current async context
        return sp => CurrentFactory is { } overrideFactory
            // 1. Return from new factory
            ? overrideFactory(sp)
            // 2. Fall back to the default implementation in the SAME scope
            : factory();
    }
    
    /// <summary>
    /// Register with a custom factory method
    /// </summary>
    /// <param name="factory">A factory function to create the service instance</param>
    /// <returns>A factory function to create the service instance</returns>
    public static Func<IServiceProvider, TService> Register(Func<IServiceProvider, TService> factory)
    {
        // Check if an override is active in the current async context
        return sp => CurrentFactory is { } overrideFactory
            // 1. Return from new factory
            ? overrideFactory(sp)
            // 2. Fall back to the default implementation in the SAME scope
            : factory(sp);
    }

}