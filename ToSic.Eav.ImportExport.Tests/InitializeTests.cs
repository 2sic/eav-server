using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Tests.Mocks;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Run;

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
            Eav.Repository.Efc.Tests.InitializeTests.ConfigureEfcDi(sc =>
            {
                sc.AddTransient<IRuntime, Runtime>();
                sc.AddTransient<IAppEnvironment, MockEnvironment>();
                sc.AddTransient<IEnvironment, MockEnvironment>();
                configure.Invoke(sc);

            }, optionalConnection);
        }
    }
}
