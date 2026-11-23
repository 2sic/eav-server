using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

/// <summary>
/// Trivial extensions to quickly/consistently get the ISysServiceProvider.
/// </summary>
public static class GetSysServiceProviderExtensions
{
    public static ISysServiceProvider GetSysServiceProvider(this ISystemServiceProviderExplicit serviceProvider)
        => serviceProvider.GetRequiredService<ISysServiceProvider>();

    /// <summary>
    /// Experimental v22 - we usually want the SysServiceProvider which is override aware.
    /// </summary>
    /// <param name="serviceScope"></param>
    /// <returns></returns>
    public static ISysServiceProvider GetSysServiceProvider(this IServiceScope serviceScope)
        => serviceScope.ServiceProvider.GetRequiredService<ISysServiceProvider>();
}
