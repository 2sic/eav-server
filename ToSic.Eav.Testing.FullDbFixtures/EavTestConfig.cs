namespace ToSic.Eav.Testing;

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
    // Net Core requires encryption to be set to false
#if NETFRAMEWORK
    // Choose between these two - the first one is the live testing-db of 2dm
    public const string DefaultConnectionString = "Data Source=localhost;Initial Catalog=eav-testing;Integrated Security=True";
    //"Data Source=localhost;Initial Catalog=eav-unit-test-temp;Integrated Security=True";
    // @"Data Source=(local);Initial Catalog=2sxc-dnn742;Integrated Security=True;";
#else
    public const string DefaultConnectionString = "Data Source=localhost;Initial Catalog=eav-testing;Integrated Security=True;Encrypt=false;TrustServerCertificate=false;";
#endif

    public const string DefaultGlobalFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";

    public const string DevMaterialsRoot = "C:\\Projects\\2sxc\\2sxc-dev-materials\\";
    public const string DevMaterialsEnd = "App_Data\\system-custom\\";

    /// <summary>
    /// Connection String to the DB
    /// </summary>
    public required string ConStr { get; init; }

    /// <summary>
    /// Global Data Folder with all the EAV/2sxc Content-Types and Settings
    /// </summary>
    public required string GlobalFolder { get; init; }

    /// <summary>
    /// Custom Data Folder for specific features/licenses etc.
    /// </summary>
    public required string GlobalDataCustomFolder { get; init; }

    /// <summary>
    /// Advanced Patrons setup with additional licenses activated etc.
    /// Requires that the developer has access to the dev-materials repository.
    /// </summary>
    public static EavTestConfig ScenarioFullPatrons => new()
    {
        ConStr = DefaultConnectionString,
        GlobalFolder = DefaultGlobalFolder,
        GlobalDataCustomFolder = DevMaterialsRoot + DevMaterialsEnd,
    };

    /// <summary>
    /// Basic setup with all the defaults and no special features / licenses activated.
    /// </summary>
    public static EavTestConfig ScenarioBasic = new()
    {
        ConStr = DefaultConnectionString,
        GlobalFolder = DefaultGlobalFolder,
        GlobalDataCustomFolder = $"{DevMaterialsRoot}ScenarioBasic\\{DevMaterialsEnd}",
    };
}