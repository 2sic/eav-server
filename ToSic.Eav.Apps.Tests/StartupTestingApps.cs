using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Apps.Tests
{
    [TestClass]
    public class StartupTestingApps
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Testing.Shared.StartupTestingShared.ConfigureEfcDi();
        }
        
    }
}
