using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Mocks.Named;

public static class StartupHelper
{
    public static IServiceCollection AddMockNamedServices(this IServiceCollection services)
        => services
            .AddKeyedTransient<IMockNamedService, MockNamedServiceAbc>(MockNamedServiceAbc.NameIdRegister)
            .AddKeyedTransient<IMockNamedService, MockNamedServiceDef>(MockNamedServiceDef.NameIdRegister)
            .AddKeyedTransient<IMockNamedService, MockNamedServiceMultiple>(MockNamedServiceMultiple.NameIdRegister)
            .AddKeyedTransient<IMockNamedService, MockNamedServiceMultipleSecond>(MockNamedServiceMultipleSecond.NameIdRegister);
}