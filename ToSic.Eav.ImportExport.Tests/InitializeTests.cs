using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Tests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Testing.Shared.InitializeTests.ConfigureEfcDi(sc =>
            {
                sc.AddTransient<IRuntime, Runtime>();
            });
        }
        
    }
}
