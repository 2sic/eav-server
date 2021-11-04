using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Tests.Mocks;
using ToSic.Eav.Data;
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
            ConfigureEfcDi(null, connectionString);
        }

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure = null, string optionalConnection = null)
        {
            Repository.Efc.Tests.StartupTestingRepository.ConfigureEfcDi(services =>
            {
                services.AddTransient<IRuntime, Runtime>();
                services.TryAddTransient<IValueConverter, MockValueConverter>();
                services.TryAddTransient<IZoneMapper, MockZoneMapper>();

                services
                    .AddEavCore()
                    .AddEavCorePlumbing()
                    .AddEavCoreFallbackServices();

                configure?.Invoke(services);

            }, optionalConnection);
        }
    }
}
