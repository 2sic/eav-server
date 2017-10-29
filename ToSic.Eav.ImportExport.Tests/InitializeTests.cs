using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.ImportExport.Tests
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
