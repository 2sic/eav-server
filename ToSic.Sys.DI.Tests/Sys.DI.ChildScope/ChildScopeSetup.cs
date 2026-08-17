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
            .BuildServiceProvider();

        var childSp = parentSp
            .AddChildScope(services =>
            {
                services.TryAddTransient<MockChildTransientBasic>();
                services.TryAddTransient<IMockTransientStandalone, MockChildTransientBasic>();
                services.TryAddScoped<MockChildScopedBasic>();

                // Re-Register previously registered services to test that they are freshly scoped
                services.TryAddScoped<MockScopedStandaloneToReRegister>();
                return services;
            });


        return (parentSp, childSp);
    }

    internal static T? ParentService<T>() where T : class => BuildSps().Parent.GetService<T>();
    internal static T? ChildService<T>() where T : class => BuildSps().Child.GetService<T>();
}

internal class MockChildTransientBasic : MockTransientStandalone
{
    private const int InitialValue = 2603;
    public override int Value { get; set; } = InitialValue;
}

internal class MockChildScopedBasic : MockScopedStandalone
{
    private const int InitialValue = 20395;
    public override int Value { get; set; } = InitialValue;
}
