using Microsoft.Extensions.DependencyInjection;

namespace ToSic.NamedDependencies;

public class Startup
{
    public virtual void ConfigureServices(IServiceCollection services) =>
        services
            .AddKeyedTransient<IMockNamedService, MockNamedServiceAbc>(MockNamedServiceAbc.NameIdRegister)
            .AddKeyedTransient<IMockNamedService, MockNamedServiceDef>(MockNamedServiceDef.NameIdRegister)
            .AddKeyedTransient<IMockNamedService, MockNamedServiceMultiple>(MockNamedServiceMultiple.NameIdRegister)
            .AddKeyedTransient<IMockNamedService, MockNamedServiceMultipleSecond>(MockNamedServiceMultipleSecond.NameIdRegister)
        ;
}