using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

public class LazySupportingOverride<T>(IServiceProvider provider) : Lazy<T>(CreateFactory(provider))
    where T : class
{
    // Reuse a static callback delegate to avoid allocations during ExecutionContext.Run
    private static readonly ContextCallback RestoreContextAndResolve = state =>
    {
        var (provider, resultHolder) = ((IServiceProvider, Box<T>))state!;
        resultHolder.Value = provider.GetRequiredService<T>();
    };

    private static Func<T> CreateFactory(IServiceProvider provider)
    {
        // 1. Single reference read (~2ns, 0 bytes allocated)
        var capturedContext = ExecutionContext.Capture();

        return () =>
        {
            // Fast Path: Active override context exists on current thread
            if (OverrideService<T>.CurrentFactory != null)
                return provider.GetRequiredService<T>();

            // Fallback Path: Deferred access outside 'using' block
            if (capturedContext == null)
                return provider.GetRequiredService<T>();
            
            var box = new Box<T>();
            // Restore context without allocating a new delegate
            ExecutionContext.Run(capturedContext, RestoreContextAndResolve, (provider, box));
            return box.Value!;

        };
    }

    private class Box<TValue> { public TValue? Value; }
}