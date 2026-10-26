using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Boot;
using ToSic.Sys.Capabilities;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Fingerprints;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Capabilities.Platform;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupSysCapabilities
{
    /// <summary>
    /// Add everything around system capabilities, incl. features, licenses and requirements.
    /// </summary>
    public static IServiceCollection AddSysCapabilities(this IServiceCollection services)
    {
        return services
            .AddSysFeatures()
            .AddSysRequirements()
            .AddSysLicenses();
    }

    /// <summary>
    /// Add system features services only.
    /// </summary>
    public static IServiceCollection AddSysFeatures(this IServiceCollection services)
    {
        // The catalogs must be singleton
        services.AddSingleton<FeaturesCatalog>();

        // New SystemCapability
        services.TryAddTransient<SysFeaturesLoader>();

        // Features - 2024-05-31 changed to non-singleton
        services.TryAddTransient<ISysFeaturesService, LibSysFeaturesService>();
        services.TryAddTransient<ILibFeaturesService, LibSysFeaturesService>();

        // v20 Startup - Registration of Licenses and Features
        services.AddTransient<IBootProcess, BootRegisterEavFeatures>();

        return services;
    }

    /// <summary>
    /// Add license services.
    /// </summary>
    public static IServiceCollection AddSysLicenses(this IServiceCollection services)
    {
        // The catalogs must be singleton
        services.AddSingleton<LicenseCatalog>();

        services.TryAddTransient<ILicenseService, LicenseService>();

        // Fingerprinting: Because fo security, we are not injecting the interface
        // As that would allow replacing the finger-printer with something else
        // We actually only use the direct object in DI
        services.TryAddTransient<SystemFingerprint>();

        // v20 Startup - Registration of Licenses and Features
        services.AddTransient<IBootProcess, BootRegisterEavLicenses>();

        return services;
    }

    /// <summary>
    /// Add requirements services.
    /// </summary>
    public static IServiceCollection AddSysRequirements(this IServiceCollection services)
    {
        // V14 Requirements Checks - don't use try-add, as we'll add many
        services.TryAddTransient<IRequirementsService, RequirementsService>();
        // V22 - moving requirement checks to keyed services
        services.AddKeyedTransientWithMarker<IRequirementCheck, FeatureRequirementCheck>(FeatureConstants.RequirementFeature);
        services.AddKeyedTransientWithMarker<IRequirementCheck, SysFeatureRequirementCheck>(FeatureConstants.RequirementSysCapability);

        return services;
    }


    public static IServiceCollection AddSysCapabilitiesFallbacks(this IServiceCollection services)
    {
        services.TryAddTransient<IPlatformInfo, PlatformUnknown>();

        return services;
    }
}