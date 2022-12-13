using ToSic.Eav.Apps;

namespace ToSic.Eav.DataSourceTests
{
    internal class TestConfig
    {
        //public static int Zone = 2;

        public static int AppForQueryTests = 4;

        public static IAppIdentity BigDataTestsApp = new AppIdentity(2, 9);

        public const string GlobalQueriesData = "..\\..\\..\\2sxc\\Src\\Data\\App_Data\\system\\queries\\";
        public const string TestingPath = "testdata";

    }
}
