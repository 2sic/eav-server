using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazySupportingOverride<T>(IServiceProvider provider) : Lazy<T>(CreateFactory(provider))
    where T : class
{
    /// <summary>
    /// Helper callback Function to get a service from a service provider.
    /// It will be called from ExecutionContext.Run, restoring the "using" context which existed when
    /// this <see cref="LazySupportingOverride{T}"/> was initially created.
    /// </summary>
    /// <remarks>
    /// Defined as a reusable static callback delegate
    /// to avoid allocations during ExecutionContext.Run later on.
    /// </remarks>
    private static readonly ContextCallback RestoreContextAndResolve = state =>
    {
        // Unpack the parameters passed in
        var (serviceProvider, resultBox) = ((IServiceProvider, ResultBox<T>))state!;

        // Get the service, and add it to the box, so the caller can extract it again.
        resultBox.Value = serviceProvider.GetRequiredService<T>();
    };

    private static Func<T> CreateFactory(IServiceProvider provider)
    {
        // 1. Single reference read (~2ns, 0 bytes allocated)
        // Will capture all AsyncLocal values, including the current override context, so that it can be restored later.
        var capturedContext = ExecutionContext.Capture();

        return () =>
        {
            // Fast Path: Active override context exists on current thread
            if (OverrideService<T>.CurrentFactory != null)
                return provider.GetRequiredService<T>();

            // Fallback Path: Deferred access outside 'using' block
            if (capturedContext == null)
                return provider.GetRequiredService<T>();
            
            // Prepare the result holder box
            var box = new ResultBox<T>();

            // Run within the restored context
            ExecutionContext.Run(capturedContext, RestoreContextAndResolve, (provider, box));
            return box.Value!;

        };
    }

    private class ResultBox<TValue> { public TValue? Value; }
}