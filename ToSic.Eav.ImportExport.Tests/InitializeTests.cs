using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.ImportExport.Tests
{
    [TestClass]
    class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
             Repository.Efc.Tests.InitializeTests.ConfigureEfcDi();
        }
        
    }
}
