using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context) => Testing.Shared.InitializeTests.ConfigureEfcDi();

    }
}
