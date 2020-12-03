using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data.ValueConverter;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Testing.Shared.Mocks;

namespace ToSic.Eav.Core.Tests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context) 
            => ConfigureEfcDi(sc => { });


        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure)
        {
            //Testing.Shared.InitializeTests.ConfigureEfcDi
            Factory.ActivateNetCoreDi(sc =>
            {
                //Trace.WriteLine("di configuration core");
                sc.TryAddTransient<IValueConverter, MockValueConverter>();
                sc.TryAddTransient<IRuntime, RuntimeUnknown>();
                sc.TryAddTransient<IFingerprint, FingerprintProvider>();
                configure.Invoke(sc);   // call parent invoker if necessary (usually not relevant at core, as this is the top-level
            });

        }
    }
}
