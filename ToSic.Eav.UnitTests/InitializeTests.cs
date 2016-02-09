using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            new Configuration().ConfigureDefaultMappings(Eav.Factory.Container);
        }
    }
}
