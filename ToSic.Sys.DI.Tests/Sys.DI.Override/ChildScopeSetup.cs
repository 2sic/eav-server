using Microsoft.Extensions.DependencyInjection;
using ToSic.Mock.LifetimeServices;

namespace ToSic.Sys.DI.Override;

public class ChildScopeSetup
{
    internal static IServiceProvider BuildSps()
    {
        var parentSp = new ServiceCollection()
            .AddMockLifetimes()
            .AddMockPreRegisterChildInstances()
            .BuildServiceProvider();
        return parentSp;
    }

    internal static T? GetService<T>() where T : class => BuildSps().GetService<T>();
}