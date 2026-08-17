using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI.ChildScope;

//public static class OverrideContext<T> where T : class
//{
//    private static readonly AsyncLocal<T?> _current = new();

//    public static T? Current => _current.Value;

//    public static IDisposable Begin(T overrideInstance)
//    {
//        // Save whatever was there before (could be null, or an outer override)
//        var previousValue = _current.Value;
//        _current.Value = overrideInstance;

//        return new Scope(previousValue);
//    }

//    private class Scope(T? previousValue) : IDisposable
//    {
//        private bool _disposed;

//        public void Dispose()
//        {
//            if (_disposed)
//                return;
            
//            // Restore the outer context when the inner scope ends!
//            _current.Value = previousValue;
//            _disposed = true;
//        }
//    }
//}

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
}