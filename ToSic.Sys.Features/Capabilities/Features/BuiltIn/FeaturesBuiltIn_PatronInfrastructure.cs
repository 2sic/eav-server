using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Sys.Capabilities.Features;

public partial class BuiltInFeatures
{
    internal static List<FeatureLicenseRule> ForPatronInfrastructureAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronInfrastructure, true);


    public static readonly Feature SqlCompressDataTimeline = new()
    {
        NameId = nameof(SqlCompressDataTimeline),
        Guid = new("87325de8-d671-4731-bd58-186ff6de6329"),
        Name = "Shrink your DB size by up to 80%",
        IsPublic = false,
        Ui = true,
        Description = "Shrink your DB size by up to 80%. Enables compressed JSON for the change-history which is rarely accessed.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronInfrastructureAutoEnabled
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
        LicenseRules = ForPatronInfrastructureAutoEnabled,

        // depending on the state of this feature, we will activate some static code enhancements
        RunOnStateChange = (state, log) =>
        {
            var isEnabled = state.IsEnabled;
            SysPerfSettings.CacheListAutoIndex = isEnabled;
            log.A($"Set {nameof(SysPerfSettings.CacheListAutoIndex)} = {SysPerfSettings.PreferArray}");
        }
    };



    public static readonly Feature LinqListOptimizations = new()
    {
        NameId = nameof(LinqListOptimizations),
        Guid = new("66c561a9-4218-4035-a172-580c54f73449"),
        Name = "C# Improve Data Processing Code (LINQ) by ca. 30% (BETA)",
        IsPublic = false,
        Ui = true,
        Description = "We invested quite some time to make 2sxc faster in many subtle ways.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronInfrastructureAutoEnabled,

        // depending on the state of this feature, we will activate some static code enhancements
        RunOnStateChange = (state, log) =>
        {
            var isEnabled = state.IsEnabled;
            SysPerfSettings.PreferArray = isEnabled;
            SysPerfSettings.OptimizeParentApp = isEnabled;
            log.A($"Set {nameof(SysPerfSettings.PreferArray)} = {SysPerfSettings.PreferArray}");
        }
    };



    public static readonly Feature DatabaseWriteOptimized = new()
    {
        NameId = nameof(DatabaseWriteOptimized),
        Guid = new("e73a3601-3fd1-454c-ad56-61bd581ed481"),
        Name = "Database performance with optimized write/save",
        IsPublic = false,
        Ui = true,
        Description = "Perform data save operations in a new optimized, parallel execution. Way fewer DB operations and up to 10x faster in large imports.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronInfrastructureAutoEnabled,
    };

    public static readonly Feature DatabaseTrackingOptimized = new()
    {
        NameId = nameof(DatabaseTrackingOptimized),
        Guid = new("dd0dbfec-7675-4f7d-9742-9e7c819cef2f"),
        Name = "Improve performance change tracking",
        IsPublic = false,
        Ui = true,
        Description = "Performance improvements in EntityFramework regarding change tracking.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronInfrastructureAutoEnabled,
    };

    // TODO: PROBABLY RENAME TO SqlPerformance
    public static readonly Feature SqlLoadPerformance = new()
    {
        NameId = nameof(SqlLoadPerformance),
        Guid = new("f63d8b5f-21aa-4fbd-8e14-5507ff6426a2"),
        Name = "Improve SQL performance accessing the data base",
        IsPublic = false,
        Ui = true,
        Description = "Improve SQL performance accessing the data base, for example when loading data. Can speed up loading data by 2x - 5x.",
        Security = FeaturesCatalogRules.Security0Neutral,
        LicenseRules = ForPatronInfrastructureAutoEnabled,
        ConfigurationContentType = "e523fd1a-f9e3-4e73-826e-1433e8afca8a", // 🥷SqlPerformance
    };

    /// <summary>
    /// Dummy class to manage the keys we want to read of the configuration
    /// </summary>
    public class SqlPerformanceConfig
    {
        public int RelationshipLoadChunking;
    }

}