using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Fingerprints;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Capabilities.Platform;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;
using SysFeaturesService = ToSic.Sys.Capabilities.SysFeatures.SysFeaturesService;

namespace ToSic.Sys;

public static class SetupSysCapabilities
{
    public static IServiceCollection AddSysCapabilities(this IServiceCollection services)
    {
        // Make sure that IFeaturesInternal and IFeatures use the same singleton!
        services.AddSingleton<LicenseCatalog>();    // Must be singleton
        services.AddSingleton<FeaturesCatalog>();   // Must be singleton

        // New SystemCapability
        services.TryAddTransient<SysFeaturesService>();

        // V14 Requirements Checks - don't use try-add, as we'll add many
        services.TryAddTransient<RequirementsService>();
        services.AddTransient<IRequirementCheck, FeatureRequirementCheck>();
        services.AddTransient<IRequirementCheck, SysFeatureRequirementCheck>();

        services.TryAddTransient<ILicenseService, LicenseService>();

        // Fingerprinting: Because fo security, we are not injecting the interface
        // As that would allow replacing the finger-printer with something else
        // We actually only use the direct object in DI
        services.TryAddTransient<SystemFingerprint>();

        return services;
    }

    public static IServiceCollection AddSysCapabilitiesFallback(this IServiceCollection services)
    {
        services.TryAddTransient<IPlatformInfo, PlatformUnknown>();

        return services;
    }
}