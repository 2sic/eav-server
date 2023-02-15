namespace ToSic.Testing.Shared
{
    /// <summary>
    /// The main configuration to use.
    ///
    /// If you have special needs, create an instance and override.
    ///
    /// @STV TODO: We should find a way (environment variable?) to return a different value on your PC for the DB etc.
    /// </summary>
    public class TestConfiguration
    {
        // Choose between these two - the first one is the live testing-db of 2dm
        public string ConStr =
            "Data Source=localhost;Initial Catalog=eav-testing;Integrated Security=True";
            //"Data Source=localhost;Initial Catalog=eav-unit-test-temp;Integrated Security=True";


        // @"Data Source=(local);Initial Catalog=2sxc-dnn742;Integrated Security=True;";
        public string GlobalFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";
        public string GlobalDataFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\App_Data\\system\\";
        public string GlobalDataCustomFolder = "C:\\Projects\\2sxc\\2sxc-dev-materials\\App_Data\\system-custom\\";


        public bool AddAllEavServices = true;

    }
}
