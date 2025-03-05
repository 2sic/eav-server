namespace ToSic.Testing.Shared;

/// <summary>
/// The main configuration to use.
///
/// If you have special needs, create an instance and override.
///
/// @STV TODO: We should find a way (environment variable?) to return a different value on your PC for the DB etc.
/// </summary>
/// <remarks>
/// Do not call it `TestConfiguration` as it causes a lot of problems with `TestConfiguration` in the Microsoft-Testing namespace.
/// </remarks>
public record EavTestConfig
{
    // Choose between these two - the first one is the live testing-db of 2dm
    public const string DefaultConnectionString = "Data Source=localhost;Initial Catalog=eav-testing;Integrated Security=True";
    //"Data Source=localhost;Initial Catalog=eav-unit-test-temp;Integrated Security=True";
    // @"Data Source=(local);Initial Catalog=2sxc-dnn742;Integrated Security=True;";

    public const string DefaultGlobalFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";

    public const string DevMaterialsRoot = "C:\\Projects\\2sxc\\2sxc-dev-materials\\";
    public const string DevMaterialsEnd = "App_Data\\system-custom\\";

    public string ConStr { get; set; }


    public string GlobalFolder { get; set; }

    public string GlobalDataCustomFolder { get; /*init;*/ /* init not yet possible */ set; }

    public static EavTestConfig ScenarioFullPatrons => new()
    {
        ConStr = DefaultConnectionString,
        GlobalFolder = DefaultGlobalFolder,
        GlobalDataCustomFolder = DevMaterialsRoot + DevMaterialsEnd,
    };

    public static EavTestConfig ScenarioBasic = new()
    {
        ConStr = DefaultConnectionString,
        GlobalFolder = DefaultGlobalFolder,
        GlobalDataCustomFolder = $"{DevMaterialsRoot}ScenarioBasic\\{DevMaterialsEnd}",
    };
}