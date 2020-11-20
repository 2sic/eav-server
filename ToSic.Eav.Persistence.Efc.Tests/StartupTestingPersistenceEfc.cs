using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Tests.Mocks;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class StartupTestingPersistenceEfc
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ConfigureEfcDi(sc => { });
        }

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure, string optionalConnection = null)
        {
            Testing.Shared.StartupTestingShared.ConfigureEfcDi(sc => {
                sc.AddTransient<ITargetTypes, MockGlobalMetadataProvider>();
                configure.Invoke(sc);
            }, optionalConnection);
        }


    }
}
