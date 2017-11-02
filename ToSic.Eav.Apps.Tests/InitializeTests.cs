using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Apps.Tests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Testing.Shared.InitializeTests.ConfigureEfcDi();
        }
        
    }
}
