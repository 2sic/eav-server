namespace ToSic.Sys.DI;

/// <summary>
/// Helper used by LazySvc and Generator.
/// </summary>
/// <typeparam name="TService"></typeparam>
internal static class GeneratorForOverridesKeyed<TService> where TService : class
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
    internal static readonly ContextCallback RestoreContextAndResolveKeyed =
        state =>
        {
            // Unpack the parameters passed in
            var (sp, resultBox) = ((IServiceProvider, KeyedSpecs))state!;
            var (key, justTry, log) = resultBox;

            // Get the service, and add it to the box, so the caller can extract it again.
            resultBox.Value = key == null
                ? sp.Build<TService>(log)
                : justTry
                    ? sp.TryBuild<TService>(key, log)!
                    : sp.Build<TService>(key, log);
        };


    internal static Func<KeyedSpecs, TService> CreateFactory(IServiceProvider sp)
    {
        // 1. Single reference read (~2ns, 0 bytes allocated)
        // Will capture all AsyncLocal values, including the current override context, so that it can be restored later.
        var capturedContext = ExecutionContext.Capture();

        // 2. Return a function holding this context,
        // which will be the factory once it's needed
        return specs =>
        {
            var (key, justTry, log) = specs;

            // V1: Fast Path: Active override context exists on current thread
            // V2: Fallback Path: Deferred access outside 'using' block, so no captured context exists, so just use the service provider directly
            if (OverrideService<TService>.CurrentFactory != null || capturedContext == null)
                return key == null
                    ? sp.Build<TService>(log)
                    : justTry
                        ? sp.TryBuild<TService>(key, log)!
                        : sp.Build<TService>(key, log);

            // Prepare the result holder box, make sure we copy it.
            var box = specs with { };

            // Create a context copy, because at least in .net 4.8 it causes problems
            // Error: Cannot apply a context that has been marshaled across AppDomains, that was not acquired through a Capture operation or that has already been the argument to a Set call.
            // Note that it doesn't seem to happen in .net 10, but we keep it for safety.
            var contextCopy = capturedContext.CreateCopy();

            // Run within the restored context
            ExecutionContext.Run(contextCopy, RestoreContextAndResolveKeyed, (sp, box));
            return box.Value!;
        };
    }

    internal record KeyedSpecs(string? Key, bool JustTry, ILog? Log)
    {
        public TService? Value { get; set; }
    }
}
