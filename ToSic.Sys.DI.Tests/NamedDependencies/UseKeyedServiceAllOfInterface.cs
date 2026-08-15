using ToSic.Mocks.Named;
using ToSic.Sys;

namespace ToSic.NamedDependencies;

/// <summary>
/// This test must run in isolation, with own startup, so it doesn't fail because
/// of mixed service providers from other tests.
/// </summary>
/// <param name="services"></param>
public class UseKeyedServiceAllOfInterface(IEnumerable<IMockNamedService> services)
{
    public class Startup() : QuickStartup(s => s.AddMockNamedServices());
    
    [Fact]
    public void WithoutNameItHasNone()
        => Empty(services);
}