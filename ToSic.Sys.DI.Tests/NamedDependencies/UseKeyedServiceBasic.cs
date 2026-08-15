using Microsoft.Extensions.DependencyInjection;
using ToSic.Mocks.Named;
using ToSic.Sys;

namespace ToSic.NamedDependencies;

public class UseKeyedServiceAbc([FromKeyedServices(MockNamedServiceAbc.NameIdConst)] IMockNamedService services)
{
    public class Startup() : QuickStartup(s => s.AddMockNamedServices());

    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceAbc.NameIdConst, services.NameId);
}


public class UseKeyedServiceDef([FromKeyedServices(MockNamedServiceDef.NameIdConst)] IMockNamedService services)
{
    public class Startup() : QuickStartup(s => s.AddMockNamedServices());

    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceDef.NameIdConst, services.NameId);
}
