using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// It's important that this does NOT implement IDisposable, since that would destroy the inner "true" service provider
/// </remarks>
public sealed class ContextualServiceProvider
    : ISysServiceProvider,
        ISystemServiceProviderExplicit, /* IServiceProvider */
        IKeyedServiceProvider,
        IServiceScopeFactory
{
    private readonly ISystemServiceProviderExplicit _inner;
    private readonly ExecutionContext? _capturedContext;
    private readonly bool _capturedHasOverrides;

    public ContextualServiceProvider(ISystemServiceProviderExplicit inner)
        : this(inner, ExecutionContext.Capture(), AmbientOverrideTracker.HasActiveOverrides)
    {
    }

    private ContextualServiceProvider(
        ISystemServiceProviderExplicit inner,
        ExecutionContext? capturedContext,
        bool capturedHasOverrides)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _capturedContext = capturedContext;
        _capturedHasOverrides = capturedHasOverrides;
    }

    public object? GetService(Type serviceType)
    {
        // Intercept nested requests for the provider or scope factory
        if (serviceType == typeof(ISystemServiceProviderExplicit)   /* IServiceProvider */
            || serviceType == typeof(IKeyedServiceProvider)
            || serviceType == typeof(IServiceScopeFactory))
            return this;

        // Self-Capture: not necessary, as it will happen automatically
        // when the underlying ServiceProvider is called
        //if (serviceType == typeof(ISysServiceProvider))
        //    return new ContextualServiceProvider(_inner);

        if (/*AmbientOverrideTracker.HasActiveOverrides ||*/ !_capturedHasOverrides || _capturedContext == null)
            return _inner.GetService(serviceType);

        var state = new ResolveState { Provider = _inner, ServiceType = serviceType };
        ExecutionContext.Run(_capturedContext.CreateCopy(), ResolveCallback, state);
        return state.Result;
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        if (_inner is not IKeyedServiceProvider keyedInner)
            throw new InvalidOperationException("Underlying provider does not support keyed services.");

        if (/*AmbientOverrideTracker.HasActiveOverrides ||*/ !_capturedHasOverrides || _capturedContext == null)
            return keyedInner.GetKeyedService(serviceType, serviceKey);

        var state = new ResolveState
        {
            Provider = _inner,
            ServiceType = serviceType,
            ServiceKey = serviceKey,
            IsKeyed = true
        };
        ExecutionContext.Run(_capturedContext.CreateCopy(), ResolveCallback, state);
        return state.Result;
    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        if (_inner is not IKeyedServiceProvider keyedInner)
            throw new InvalidOperationException("Underlying provider does not support keyed services.");

        if (/*AmbientOverrideTracker.HasActiveOverrides ||*/ !_capturedHasOverrides || _capturedContext == null)
            return keyedInner.GetRequiredKeyedService(serviceType, serviceKey);

        var state = new ResolveState
        {
            Provider = _inner,
            ServiceType = serviceType,
            ServiceKey = serviceKey,
            IsKeyed = true,
            IsRequired = true
        };
        ExecutionContext.Run(_capturedContext.CreateCopy(), ResolveCallback, state);
        return state.Result!;
    }

    public IServiceScope CreateScope()
    {
        var innerScope = _inner.CreateScope();
        return new ContextualServiceScope(innerScope, _capturedContext, _capturedHasOverrides);
    }

    private static readonly ContextCallback ResolveCallback = static s =>
    {
        var state = (ResolveState)s!;
        state.Result = (state.IsKeyed, state.IsRequired) switch
        {
            (false, _) => state.Provider.GetService(state.ServiceType),
            (true, false) => ((IKeyedServiceProvider)state.Provider).GetKeyedService(state.ServiceType, state.ServiceKey),
            (true, true) => ((IKeyedServiceProvider)state.Provider).GetRequiredKeyedService(state.ServiceType, state.ServiceKey)
        };
    };

    private sealed class ResolveState
    {
        public required ISystemServiceProviderExplicit Provider { get; init; }
        public required Type ServiceType { get; init; }
        public object? ServiceKey { get; init; }
        public bool IsKeyed { get; init; }
        public bool IsRequired { get; init; }
        public object? Result { get; set; }
    }

    private sealed class ContextualServiceScope(
        IServiceScope innerScope,
        ExecutionContext? capturedContext,
        bool capturedHasOverrides)
        : IServiceScope, IAsyncDisposable
    {
        public ISystemServiceProviderExplicit ServiceProvider { get; } = new ContextualServiceProvider(
            innerScope.ServiceProvider,
            capturedContext,
            capturedHasOverrides);

        public void Dispose() => innerScope.Dispose();

        public ValueTask DisposeAsync()
        {
            if (innerScope is IAsyncDisposable asyncDisposable)
                return asyncDisposable.DisposeAsync();

            innerScope.Dispose();
            return default;
        }
    }
}