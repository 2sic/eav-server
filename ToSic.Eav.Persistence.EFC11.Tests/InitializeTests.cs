using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Testing.Shared.InitializeTests.ConfigureEfcDi(sc => { });
        }
        
    }
}
