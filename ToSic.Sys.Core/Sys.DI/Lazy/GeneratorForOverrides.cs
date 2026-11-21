namespace ToSic.Sys.DI;

/// <summary>
/// Helper used by LazySvc and Generator.
/// </summary>
/// <typeparam name="TService"></typeparam>
internal static class GeneratorForOverrides<TService> where TService : class
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
    internal static readonly ContextCallback RestoreContextAndResolve =
        state =>
        {
            // Unpack the parameters passed in
            var (serviceProvider, resultBox) = ((IServiceProvider, ResultBoxBasic<TService>))state!;

            // Get the service, and add it to the box, so the caller can extract it again.
            resultBox.Value = serviceProvider.Build<TService>(resultBox.Log);
        };

    internal static Func<ILog?, TService> CreateFactory(IServiceProvider provider)
    {
        // 1. Single reference read (~2ns, 0 bytes allocated)
        // Will capture all AsyncLocal values, including the current override context, so that it can be restored later.
        var capturedContext = ExecutionContext.Capture();

        // 2. Return a function holding this context,
        // which will be the factory once it's needed
        return log =>
        {
            // Fast Path: Active override context exists on current thread
            if (OverrideService<TService>.CurrentFactory != null)
                return provider.Build<TService>(log);

            // Fallback Path: Deferred access outside 'using' block
            if (capturedContext == null)
                return provider.Build<TService>(log);

            // Prepare the result holder box
            var box = new ResultBoxBasic<TService> { Log = log };

            // Create a context copy, because at least in .net 4.8 it causes problems
            // Error: Cannot apply a context that has been marshaled across AppDomains, that was not acquired through a Capture operation or that has already been the argument to a Set call.
            // Note that it doesn't seem to happen in .net 10, but we keep it for safety.
            var contextCopy = capturedContext.CreateCopy();

            // Run within the restored context
            ExecutionContext.Run(contextCopy, RestoreContextAndResolve, (provider, box));
            return box.Value!;

        };
    }

    private record ResultBoxBasic<TValue>
    {
        public TValue? Value;
        public ILog? Log;
    }
}
