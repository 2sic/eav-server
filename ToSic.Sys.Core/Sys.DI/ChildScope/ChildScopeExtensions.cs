using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI.ChildScope;

public static class ChildScopeExtensions
{
    public static IServiceProvider AddChildScope(
        this IServiceProvider parent,
        Func<IServiceCollection, IServiceCollection> configureOverrides)
    {
        var overrideServices = new ServiceCollection();
        var configured = configureOverrides(overrideServices);
        return new ChildScopeServiceProvider(parent, configured);
    }
}
