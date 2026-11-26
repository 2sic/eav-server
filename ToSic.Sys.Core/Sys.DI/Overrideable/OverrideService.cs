using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

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

public static class OverrideService<TService> where TService : class
{
    /// <summary>
    /// Key object 
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object Key = new();

    public static int OverrideCount => AmbientOverrideTracker.Count(Key);

    public static string OverridePath => AmbientOverrideTracker.GetPath(Key);

    public static Func<ISysServiceProvider, TService>? CurrentFactory =>
        AmbientOverrideTracker.Find<Func<ISysServiceProvider, TService>>(Key);



    #region Registrations

    public static Func<IServiceProvider, TService> Register<TDefault>()
        where TDefault : class, TService =>
        sp =>
        {
            var sysSp = sp.GetRequiredService<ISysServiceProvider>();
            return CurrentFactory?.Invoke(sysSp) ?? sysSp.GetRequiredService<TDefault>();
        };

    public static Func<ISystemServiceProviderExplicit, TService> Register(Func<TService> fallbackFactory)
    {
        return fallbackFactory is null
            ? throw new ArgumentNullException(nameof(fallbackFactory))
            : Register(_ => fallbackFactory());
    }

    public static Func<IServiceProvider, TService> Register(Func<ISysServiceProvider, TService> fallbackFactory)
    {
        if (fallbackFactory is null)
            throw new ArgumentNullException(nameof(fallbackFactory));

        return sp =>
        {
            var sysSp = sp.GetRequiredService<ISysServiceProvider>();
            return CurrentFactory?.Invoke(sysSp) ?? fallbackFactory(sysSp);
        };
    }

    #endregion



    #region Use

    public static IDisposable Use(
        TService instance,
        string? hint = null,
        [CallerMemberName] string? callerName = null) =>
        Use(_ => instance, hint, callerName);

    public static IDisposable Use<TImplementation>(
        string? hint = null,
        [CallerMemberName] string? callerName = null)
        where TImplementation : class, TService =>
        Use(static sp => sp.GetRequiredService<TImplementation>(), hint, callerName);

    public static IDisposable Use(
        Func<ISysServiceProvider, TService> factory,
        string? hint = null,
        [CallerMemberName] string? callerName = null) =>
        AmbientOverrideTracker.Push(Key, factory, callerName, hint);

    #endregion

}
