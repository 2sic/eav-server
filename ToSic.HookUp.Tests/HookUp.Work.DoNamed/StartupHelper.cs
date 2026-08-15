using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.HookUp;

namespace ToSic.HookUp.Work.DoNamed;

internal static class StartupHelper
{
    public static IServiceCollection AddMockNamedServices(this IServiceCollection services)
        => services
            .AddKeyedTransient<IWork<string, string>, MockNamedBefore>(MockNamedBefore.PhaseName)
            .AddKeyedTransient<IWork<string, string>, MockNamedAfter>(MockNamedAfter.PhaseName);
}