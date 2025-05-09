using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Requirements;
using ToSic.Lib.Internal.FeatSys.Features;

namespace ToSic.Lib.Internal.FeatSys;

public static class StartupLibFeatSys
{
    public static IServiceCollection AddLibFeatSys(this IServiceCollection services)
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

        return services;
    }

}