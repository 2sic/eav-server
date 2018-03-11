using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc.Tests.Mocks;
using ToSic.SexyContent.ImportExport;

namespace ToSic.Eav.Repository.Efc.Tests
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
            Persistence.Efc.Tests.InitializeTests.ConfigureEfcDi(sc =>
            {
                // these are only used in Repository.Efc.Tests
                sc.AddTransient<Apps.ImportExport.XmlExporter, DnnXmlExporter>();

                sc.AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>();

                configure.Invoke(sc);
            }, optionalConnection);
            Factory.Debug = true;


        }
    }
}
