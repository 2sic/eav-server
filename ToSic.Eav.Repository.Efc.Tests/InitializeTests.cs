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
            Testing.Shared.InitializeTests.ConfigureEfcDi(sc =>
            {
                // these are only used in Repository.Efc.Tests
                sc.AddTransient<ToSic.Eav.Apps.ImportExport.XmlExporter, ToSxcXmlExporter>();//(new InjectionConstructor(0, 0, true, new string[0], new string[0]));

                sc.AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>();
            });
            Factory.Debug = true;
        }
        

    }
}
