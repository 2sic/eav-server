using Microsoft.Extensions.DependencyInjection;
using ToSic.Mocks.Named;

namespace ToSic.NamedDependencies;

/// <summary>
/// This test must run in isolation, with own startup, so it doesn't fail because
/// of mixed service providers from other tests.
/// </summary>
/// <param name="services"></param>
public class UseKeyedServiceAllOfInterface(IEnumerable<IMockNamedService> services)
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) => services.AddMockNamedServices();
    }
    
    [Fact]
    public void WithoutNameItHasNone()
        => Empty(services);
}