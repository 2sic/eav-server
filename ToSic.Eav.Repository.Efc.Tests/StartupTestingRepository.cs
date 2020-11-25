using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Tests.Mocks;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc.Tests.Mocks;
using ToSic.Eav.Run;
using ToSic.Sxc.Dnn.ImportExport;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class StartupTestingRepository
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ConfigureEfcDi(sc => { });
        }

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure, string optionalConnection = null)
        {
            Persistence.Efc.Tests.StartupTestingPersistenceEfc.ConfigureEfcDi(sc =>
            {
                // these are only used in Repository.Efc.Tests
                sc.AddTransient<Apps.ImportExport.XmlExporter, DnnXmlExporter>();

                sc.AddTransient<IRuntime, Runtime>();
                sc.AddTransient<IEnvironment, MockEnvironment>();
                sc.AddTransient<IGetDefaultLanguage, MockGetLanguage>();

                sc.AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>();

                configure.Invoke(sc);
            }, optionalConnection);
            Factory.Debug = true;


        }
    }
}
