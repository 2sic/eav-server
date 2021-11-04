using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Tests.Mocks;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc.Tests.Mocks;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Sxc.Dnn.ImportExport;
using ToSic.Testing.Shared.Mocks;

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
            ConfigureEfcDiOld(sc =>
            {
                // these are only used in Repository.Efc.Tests
                sc.AddTransient<Apps.ImportExport.XmlExporter, DnnXmlExporter>();

                sc.AddTransient<IRuntime, Runtime>();
                sc.AddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();

                sc.AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>();

                configure.Invoke(sc);
            }, optionalConnection);
            Factory.Debug = true;


        }

        public static void ConfigureEfcDiOld(Factory.ServiceConfigurator configure, string optionalConnection = null)
        {
            Testing.Shared.StartupTestingShared.ConfigureEfcDi(sc => {
                sc.AddTransient<ITargetTypes, MockGlobalMetadataProvider>();
                configure.Invoke(sc);
            }, optionalConnection);
        }

    }
}
