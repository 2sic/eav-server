using Microsoft.Extensions.DependencyInjection;
using ToSic.Mocks.Named;

namespace ToSic.NamedDependencies;

public class UseKeyedServiceAbc([FromKeyedServices(MockNamedServiceAbc.NameIdConst)] IMockNamedService services)
{
    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceAbc.NameIdConst, services.NameId);
}


public class UseKeyedServiceDef([FromKeyedServices(MockNamedServiceDef.NameIdConst)] IMockNamedService services)
{
    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceDef.NameIdConst, services.NameId);
}
