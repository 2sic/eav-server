using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    public class InitializeTests
    {
        public const string connectionForTests =
            "Data Source=(local)\\sqlexpress;Initial Catalog=eav-testing;Integrated Security=True";

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context) => Testing.Shared.InitializeTests.ConfigureEfcDi(connectionForTests);

    }
}
