using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.Mocks;
using ToSic.Eav.Implementations.Runtime;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Core.Tests
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ConfigureEfcDi(sc => { });
        }


        private static void ConfigureEfcDi(Factory.ServiceConfigurator configure)
        {
            Factory.ActivateNetCoreDi(sc =>
            {
                sc.AddTransient<IRuntime, NeutralRuntime>();
                //sc.AddTransient<IEavValueConverter, MockValueConverter>();
                //sc.AddTransient<IEavUserInformation, NeutralEavUserInformation>();
                //sc.AddTransient<IEavUserInformation, NeutralEavUserInformation>();

                configure.Invoke(sc);
            });

        }
    }
}
