using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Tests.Mocks;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ConfigureEfcDi(sc => { });
        }

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure, string optionalConnection = null)
        {
            Testing.Shared.InitializeTests.ConfigureEfcDi(sc => {
                sc.AddTransient<ITargets, MockGlobalMetadataProvider>();
                configure.Invoke(sc);
            }, optionalConnection);
        }


    }
}
