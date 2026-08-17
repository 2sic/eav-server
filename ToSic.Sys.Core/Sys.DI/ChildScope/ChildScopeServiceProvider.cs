using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI.ChildScope;

public class ChildScopeServiceProvider : IServiceProvider, IDisposable
{
    private readonly IServiceProvider _parent;
    private readonly Dictionary<Type, ServiceDescriptor> _descriptors;
    private readonly ConcurrentDictionary<ServiceDescriptor, object?> _scopedCache = new();
    private readonly List<IDisposable> _disposables = [];

    public ChildScopeServiceProvider(IServiceProvider parent, IServiceCollection overrideServices)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        // Map types (last registered descriptor wins, matching default .NET behavior)
        _descriptors = overrideServices.ToDictionary(d => d.ServiceType, d => d);
    }

    public object? GetService(Type serviceType)
    {
        if (_descriptors.TryGetValue(serviceType, out var descriptor))
        {
            // Transient: Always create a new instance
            return descriptor.Lifetime == ServiceLifetime.Transient
                ? CreateInstance(descriptor)
                // Scoped / Singleton: Cache instance for the duration of this ChildScopeServiceProvider
                : _scopedCache.GetOrAdd(descriptor, CreateInstance);
        }

        // Fall back to the parent scope for any non-overridden dependency
        return _parent.GetService(serviceType);
    }

    private object? CreateInstance(ServiceDescriptor descriptor)
    {
        object? instance = null;

        if (descriptor.ImplementationInstance != null)
        {
            instance = descriptor.ImplementationInstance;
        }
        else if (descriptor.ImplementationFactory != null)
        {
            // Pass 'this' so factory delegate resolutions check overrides first
            instance = descriptor.ImplementationFactory(this);
        }
        else if (descriptor.ImplementationType != null)
        {
            // Use ActivatorUtilities passing 'this' as the container
            instance = ActivatorUtilities.CreateInstance(this, descriptor.ImplementationType);
        }

        // Track disposable instances created specifically by this provider
        if (instance is IDisposable disposable && descriptor.ImplementationInstance == null)
        {
            lock (_disposables)
            {
                _disposables.Add(disposable);
            }
        }

        return instance;
    }

    public void Dispose()
    {
        lock (_disposables)
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
            _disposables.Clear();
        }
    }
}