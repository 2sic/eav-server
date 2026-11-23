using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

public static class ContextualServiceProviderRegistration
{
    public static void AddMyModule(this IServiceCollection services, Action<IServiceCollection> configure)
    {
        // 1. Count how many services are already registered,
        // so we can only rewrite the ones added by this module
        var startIndex = services.Count;

        // 2. Register all of your module's services here
        configure(services);

        // 3. Only rewrite the descriptors added by this module
        services.UseContextualServiceProviders(startIndex);
    }

    public static void UseContextualServiceProviders(this IServiceCollection services, int startIndex)
    {
        for (var i = startIndex; i < services.Count; i++)
        {
            var d = services[i];
            if (d.ServiceType.IsGenericTypeDefinition)
                continue;

            if (d.ImplementationFactory is { } factory)
            {
                services[i] = ServiceDescriptor.Describe(
                    d.ServiceType, sp => factory(new ContextualServiceProvider(sp)), d.Lifetime);
            }
            else if (d.ImplementationType is { } type &&
                     type.GetConstructors().Any(c =>
                         c.GetParameters().Any(p => p.ParameterType == typeof(ISystemServiceProviderExplicit))))
            {
                services[i] = ServiceDescriptor.Describe(
                    d.ServiceType, sp => ActivatorUtilities.CreateInstance(new ContextualServiceProvider(sp), type),
                    d.Lifetime);
            }
        }
    }
}