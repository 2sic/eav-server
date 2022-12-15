namespace ToSic.Testing.Shared
{
    public class TestConstants
    {
        public const string ConStr =
        //     @"Data Source=.\SQLExpress;Initial Catalog=2flex 2Sexy Content;Integrated Security=True";
        // @"Data Source=localhost;Initial Catalog=2sxc-dnn742;Integrated Security=True;";
        
            // Choose between these two - the first one is the live testing-db of 2dm
        "Data Source=localhost;Initial Catalog=eav-testing;Integrated Security=True";
        //"Data Source=localhost;Initial Catalog=eav-unit-test-temp;Integrated Security=True";


        // @"Data Source=(local);Initial Catalog=2sxc-dnn742;Integrated Security=True;";
        public const string GlobalFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";
        public const string GlobalDataFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\App_Data\\system\\";
        public const string GlobalDataCustomFolder = "C:\\Projects\\2sxc\\2sxc-dev-materials\\App_Data\\system-custom\\";
    }
}
