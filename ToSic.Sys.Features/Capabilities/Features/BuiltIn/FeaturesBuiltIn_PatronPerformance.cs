using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Sys.Capabilities.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForPatronPerformanceAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronPerformance, true);


    public static readonly Feature CSharpLinqOptimizations = new()
    {
        NameId = nameof(CSharpLinqOptimizations),
        Guid = new("66c561a9-4218-4035-a172-580c54f73449"),
        Name = "C# Improve Data Processing Code (LINQ) by ca. 30% (BETA)",
        IsPublic = false,
        Ui = true,
        Description = "We invested quite some time to make 2sxc faster in many subtle ways.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronPerformanceAutoEnabled,

        // depending on the state of this feature, we will activate some static code enhancements
        RunOnStateChange = (state, log) =>
        {
            var isEnabled = state.IsEnabled;
            SysPerfSettings.PreferArray = isEnabled;
            SysPerfSettings.OptimizeParentApp = isEnabled;
            log.A($"Set {nameof(SysPerfSettings.PreferArray)} = {SysPerfSettings.PreferArray}");
        }
    };



}