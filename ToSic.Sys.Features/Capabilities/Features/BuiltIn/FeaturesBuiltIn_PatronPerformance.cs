using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Sys.Capabilities.Features;

public partial class  BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForPatronPerformanceAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronPerformance, true);


    public static readonly Feature LinqListOptimizations = new()
    {
        NameId = nameof(LinqListOptimizations),
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

    public static readonly Feature CacheListAutoIndex = new()
    {
        NameId = nameof(CacheListAutoIndex),
        Guid = new("f64be738-f999-4f15-9abd-418370000855"),
        Name = "Cache Lists - Auto-Index for faster item retrieval",
        IsPublic = false,
        Ui = true,
        Description = "This optimization improves access to specific items which are retrieved by ID or GUID.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronPerformanceAutoEnabled,

        // depending on the state of this feature, we will activate some static code enhancements
        RunOnStateChange = (state, log) =>
        {
            var isEnabled = state.IsEnabled;
            SysPerfSettings.CacheListAutoIndex = isEnabled;
            log.A($"Set {nameof(SysPerfSettings.CacheListAutoIndex)} = {SysPerfSettings.PreferArray}");
        }
    };

    // TODO: guid not final!
    public static readonly Feature DataImportParallel = new()
    {
        NameId = nameof(DataImportParallel),
        Guid = new("f64be738-0000-4f15-0000-418370000855"),
        Name = "DataImportParallel TODO",
        IsPublic = false,
        Ui = true,
        Description = "DataImportParallel TODO.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronPerformanceAutoEnabled,

        //// depending on the state of this feature, we will activate some static code enhancements
        //RunOnStateChange = (state, log) =>
        //{
        //    var isEnabled = state.IsEnabled;
        //    SysPerfSettings.CacheListAutoIndex = isEnabled;
        //    log.A($"Set {nameof(SysPerfSettings.CacheListAutoIndex)} = {SysPerfSettings.PreferArray}");
        //}
    };

}