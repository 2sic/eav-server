using Microsoft.Extensions.DependencyInjection;
using ToSic.Mocks.Named;

namespace ToSic.NamedDependencies;

public class UseKeyedServiceMultipleUsesLast([FromKeyedServices(MockNamedServiceMultiple.NameIdConst)] IMockNamedService services)
{
    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceMultipleSecond.NameIdConst, services.NameId);
}


public class UseKeyedServiceMultipleAll([FromKeyedServices(MockNamedServiceMultiple.NameIdConst)] IEnumerable<IMockNamedService> services)
{
    [Fact]
    public void NameMatches()
        => Equal(MockNamedServiceMultiple.NameIdConst, services.First().NameId);

    [Fact]
    public void HasTwo()
        => Equal(2, services.Count());
}