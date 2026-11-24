using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.DI;
using ToSic.Sys.Requirements;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Features.Requirements;

public class RegisteredCheckers
{
    [Fact]
    public void NoRegistrations_HasNoCheckers()
    {
        var sp = new ServiceCollection()
            .AddSysCoreDiAllTac()
            .BuildServiceProvider();

        // If no requirement checkers are registered, GetAllKeysForService should return an empty enumerable.
        Empty(sp.GetService<IAllKeyedServices<string, IRequirementCheck>>()!);
    }


    [Fact]
    public void ByDefault_Has2Checkers()
    {
        var sc = new ServiceCollection();
        sc.AddSysCapabilitiesAndSysCore();
        var sp = sc.BuildServiceProvider();
        
        Equal(SysCapabilitiesTestConstants.RequirementChecksInDiByDefault,
            sp.GetService<IAllKeyedServices<string, IRequirementCheck>>()!.Keys.Count());
    }
}