using Microsoft.Extensions.DependencyInjection;
using ToSic.Mocks.Named;

namespace ToSic.NamedDependencies;

public class Startup
{
    public virtual void ConfigureServices(IServiceCollection services) =>
        services.AddMockNamedServices();
}