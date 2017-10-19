using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Apps.Tests
{
    [TestClass]
    class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Testing.Shared.InitializeTests.ConfigureEfcDi();
        }
        
    }
}
