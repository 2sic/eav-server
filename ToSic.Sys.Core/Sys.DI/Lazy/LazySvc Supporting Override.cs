using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

/// <summary>
/// Helps us create lazy **Service** objects.
/// </summary>
/// <typeparam name="TService">Service type, ideally based on <see cref="ServiceBase"/></typeparam>
/// <param name="sp">Service provider, in case we need to debug something</param>
/// <remarks>
/// It has some special features to reduce the amount of plumbing in code:
/// 
/// * It will automatically lazy-attach a logger when used correctly
/// * It can also be configured with a lazy init function to keep code clean.
///
/// LazySvc will detect if the provided object/service supports these features.
/// So if it's used for anything that doesn't support logging it will just behave like `Lazy`.
///
/// Notes
/// 
/// * Constructor should never be called as it's only meant to be used with Dependency Injection.
/// * For testing, use the `LazySvcInjectable` in the TestHelpers project, which allows you to provide a pre-created object.
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazySvc<TService>(IServiceProvider sp)
    : ILazyLike<TService>,
        IHasLog,
        ILazyInitLog
    where TService : class
{
    private readonly Func<ILog?, TService> _factory = CreateFactory(sp);

    #region Get / New / Value

    public virtual TService Value => _valueGet.Get(() => _factory(Log))!;
    private readonly LazyGet<TService> _valueGet = new();

    public bool IsValueCreated => _valueGet.IsValueCreated;

    #endregion


    /// <summary>
    /// Helper callback Function to get a service from a service provider.
    /// It will be called from ExecutionContext.Run, restoring the "using" context which existed when
    /// this <see cref="LazySupportingOverride{T}"/> was initially created.
    /// </summary>
    /// <remarks>
    /// Defined as a reusable static callback delegate
    /// to avoid allocations during ExecutionContext.Run later on.
    /// </remarks>
    private static readonly ContextCallback RestoreContextAndResolve =
        state =>
        {
            // Unpack the parameters passed in
            var (serviceProvider, resultBox) = ((IServiceProvider, ResultBox<TService>))state!;

            // Get the service, and add it to the box, so the caller can extract it again.
            resultBox.Value = serviceProvider.GetRequiredService<TService>();
        };

    private static Func<ILog?, TService> CreateFactory(IServiceProvider provider)
    {
        // 1. Single reference read (~2ns, 0 bytes allocated)
        // Will capture all AsyncLocal values, including the current override context, so that it can be restored later.
        var capturedContext = ExecutionContext.Capture();

        // 2. Return a function holding this context,
        // which will be the factory once it's needed
        return (log) =>
        {
            // Fast Path: Active override context exists on current thread
            if (OverrideService<TService>.CurrentFactory != null)
                return provider.Build<TService>(log);

            // Fallback Path: Deferred access outside 'using' block
            if (capturedContext == null)
                return provider.Build<TService>(log);
            
            // Prepare the result holder box
            var box = new ResultBox<TService>();

            // Run within the restored context
            ExecutionContext.Run(capturedContext, RestoreContextAndResolve, (provider, box));
            return box.Value!;

        };
    }

    private class ResultBox<TValue> { public TValue? Value; }


    #region Logging

    /// <inheritdoc/>
    void ILazyInitLog.SetLog(ILog? parentLog)
        => Log = parentLog;

    /// <summary>
    /// The parent log, which is attached to newly generated objects
    /// _if_ they support logging.
    /// </summary>
    public ILog? Log { get; private set; }

    #endregion

}

[PrivateApi("Test-Class for comparisons, not meant for production.")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazySupportingOverride<TService>(IServiceProvider sp)
    : LazySvc<TService>(sp) where TService : class;
