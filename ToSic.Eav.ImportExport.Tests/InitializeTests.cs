using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Tests.Mocks;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.Interfaces;

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
        public static void AssemblyInit(TestContext context, string connectionString = null)
        {
            Testing.Shared.InitializeTests.ConfigureEfcDi(sc =>
            {
                sc.AddTransient<IRuntime, Runtime>();
                sc.AddTransient<IEnvironment, MockEnvironment>();
            }, connectionString);
        }        
    }
}
