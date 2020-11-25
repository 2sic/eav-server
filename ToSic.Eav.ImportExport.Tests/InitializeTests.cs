using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Tests.Mocks;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Run;
using ToSic.Testing.Shared.Mocks;

namespace ToSic.Eav.ImportExport.Tests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            AssemblyInit(context, null);
        }
        public static void AssemblyInit(TestContext context, string connectionString)
        {
            ConfigureEfcDi(sc => { }, connectionString);
        }

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure, string optionalConnection = null)
        {
            Repository.Efc.Tests.StartupTestingRepository.ConfigureEfcDi(sc =>
            {
                sc.AddTransient<IRuntime, Runtime>();
                sc.AddTransient<IGetDefaultLanguage, MockGetLanguage>();
                sc.TryAddTransient<IValueConverter, MockValueConverter>();
                sc.TryAddTransient<IZoneMapper, MockZoneMapper>();
                configure.Invoke(sc);

            }, optionalConnection);
        }
    }
}
