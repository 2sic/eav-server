namespace ToSic.Eav.Testing;

public class ScenarioConstants
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
}