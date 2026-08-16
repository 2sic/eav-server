using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.HookUp;

namespace ToSic.HookUp.MockWork;

internal static class StartupHelper
{
    public static IServiceCollection AddMockNamedServices(this IServiceCollection services)
    {
        services.TryAddTransient<MockWorkStringAddWorld>();
        services.TryAddTransient<MockWorkStringLength>();
        return services
            .AddKeyedTransient<IWork<string, string>, MockWorkNamedBefore>(MockWorkNamedBefore.PhaseName)
            .AddKeyedTransient<IWork<string, string>, MockWorkNamedAfter>(MockWorkNamedAfter.PhaseName);
    }
}