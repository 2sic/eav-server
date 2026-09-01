using Microsoft.Extensions.DependencyInjection;
using ToSic.Mocks.Named;
using ToSic.Sys;

namespace ToSic.NamedDependencies;

public class UseKeyedServiceMultipleUsesLast([FromKeyedServices(MockNamedServiceMultiple.NameIdConst)] IMockNamedService services)
{
    public class Startup() : QuickStartup(s => s.AddMockNamedServices());

    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceMultipleSecond.NameIdConst, services.NameId);
}


public class UseKeyedServiceMultipleAll([FromKeyedServices(MockNamedServiceMultiple.NameIdConst)] IEnumerable<IMockNamedService> services)
{
    public class Startup() : QuickStartup(s => s.AddMockNamedServices());

    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceMultiple.NameIdConst, services.First().NameId);

    [Fact]
    public void HasTwo()
        => Equal(2, services.Count());
}