using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSourceTests
{
    [TestClass]
    public class InitializeTests
    {
        public const string connectionForTests =
            "Data Source=srv-devdb-01;Initial Catalog=eav-testing;Integrated Security=True";

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context) =>
            Eav.ImportExport.Tests.InitializeTests.AssemblyInit(context, connectionForTests);

    }
}
