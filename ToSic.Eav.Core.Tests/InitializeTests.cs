using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.Mocks;
using ToSic.Eav.Implementations.Runtime;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;

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
                sc.TryAddTransient<IEavValueConverter, MockValueConverter>();
                sc.TryAddTransient<IRuntime, NeutralRuntime>();
                sc.TryAddTransient<IFingerprintProvider, FingerprintProvider>();
                configure.Invoke(sc);   // call parent invoker if necessary (usually not relevant at core, as this is the top-level
            });

        }
    }
}
