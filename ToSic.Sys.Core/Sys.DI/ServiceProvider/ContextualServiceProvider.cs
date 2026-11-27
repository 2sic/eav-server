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
    private readonly AmbientOverrideTracker.Entry? _capturedOverrides;

    public ContextualServiceProvider(ISystemServiceProviderExplicit inner)
        : this(inner, AmbientOverrideTracker.Current)
    {
    }

    private ContextualServiceProvider(
        ISystemServiceProviderExplicit inner,
        AmbientOverrideTracker.Entry? capturedOverrides)
    {
        _inner = inner;
        _capturedOverrides = capturedOverrides;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(ISystemServiceProviderExplicit) ||
            serviceType == typeof(IKeyedServiceProvider) ||
            serviceType == typeof(IServiceScopeFactory))
        {
            return this;
        }

        if (serviceType == typeof(ISysServiceProvider))
        {
            var merged = AmbientOverrideTracker.Merge(_capturedOverrides, AmbientOverrideTracker.Current);
            return new ContextualServiceProvider(_inner, merged);
        }

        return ExecuteWithOverrides(_inner, static (p, t) => p.GetService(t), serviceType);
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        if (_inner is not IKeyedServiceProvider keyedInner)
            throw new InvalidOperationException("This service provider doesn't support keyed services.");

        return ExecuteWithOverrides(
            keyedInner,
            static (p, args) => p.GetKeyedService(args.serviceType, args.serviceKey),
            (serviceType, serviceKey));
    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        if (_inner is not IKeyedServiceProvider keyedInner)
            throw new InvalidOperationException("This service provider doesn't support keyed services.");

        return ExecuteWithOverrides(
            keyedInner,
            static (p, args) => p.GetRequiredKeyedService(args.serviceType, args.serviceKey),
            (serviceType, serviceKey));
    }

    public ContextualServiceScope CreateScope()
    {
        var merged = AmbientOverrideTracker.Merge(_capturedOverrides, AmbientOverrideTracker.Current);
        return new(_inner.CreateScope(), merged);
    }

    IServiceScope IServiceScopeFactory.CreateScope() => CreateScope();

    private TResult ExecuteWithOverrides<TProvider, TState, TResult>(
        TProvider provider,
        Func<TProvider, TState, TResult> resolve,
        TState state)
    {
        if (_capturedOverrides is null)
            return resolve(provider, state);

        var current = AmbientOverrideTracker.Current;
        var effective = current is null ? _capturedOverrides : AmbientOverrideTracker.Merge(current, _capturedOverrides);

        AmbientOverrideTracker.Current = effective;
        try
        {
            return resolve(provider, state);
        }
        finally
        {
            AmbientOverrideTracker.Current = current;
        }
    }

    public sealed class ContextualServiceScope : IServiceScope, IAsyncDisposable
    {
        private readonly IServiceScope _innerScope;

        public ISysServiceProvider ServiceProvider { get; }
        ISystemServiceProviderExplicit IServiceScope.ServiceProvider => ServiceProvider;

        internal ContextualServiceScope(
            IServiceScope innerScope,
            AmbientOverrideTracker.Entry? capturedOverrides)
        {
            _innerScope = innerScope;
            ServiceProvider = new ContextualServiceProvider(innerScope.ServiceProvider, capturedOverrides);
        }

        public void Dispose() => _innerScope.Dispose();

        public ValueTask DisposeAsync() => _innerScope is IAsyncDisposable asyncDisposable
            ? asyncDisposable.DisposeAsync()
            : new(Task.Run(_innerScope.Dispose));
    }
}