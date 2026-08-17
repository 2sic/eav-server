using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Mock.LifetimeServices;

namespace ToSic.Sys.DI.ChildScope;

public class ChildScopeSetup
{
    internal static (IServiceProvider Parent, IServiceProvider Child) BuildSps()
    {
        var parentSp = new ServiceCollection()
            .AddMockLifetimes()
            .AddMockPreRegisterChildInstances()
            .BuildServiceProvider();

        var childSp = parentSp
            .AddChildScope(services =>
            {
                // Add services that are only registered in the child scope
                services.TryAddTransient<MockChildScopeOnlyTransientBasic>();
                services.TryAddTransient<IMockTransientStandalone, MockChildScopeOnlyTransientBasic>();
                services.TryAddScoped<MockChildScopeOnlyScopedBasic>();

                // Re-Register previously registered services to test that they are freshly scoped
                services.TryAddScoped<MockScopedToReRegisterReqITransient>();
                return services;
            });


        return (parentSp, childSp);
    }

    internal static T? ParentService<T>() where T : class => BuildSps().Parent.GetService<T>();
    internal static T? ChildService<T>() where T : class => BuildSps().Child.GetService<T>();
}