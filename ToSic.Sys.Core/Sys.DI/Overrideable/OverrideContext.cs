using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;


public static partial class OverrideContext<TService> where TService : class
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