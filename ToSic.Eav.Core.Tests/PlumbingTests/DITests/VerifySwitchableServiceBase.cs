using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests
{
    public abstract class VerifySwitchableServiceBase : TestBaseDiRaw
    {
        //protected VerifySwitchableServiceBase()
        //{
        //    SetupServices(ServiceCollection);
        //}

        //protected void SetupServices(IServiceCollection services)
        //{
        //    // base.SetupServices(services);
        //    services.AddTransient<ITestSwitchableService, TestSwitchableFallback>();
        //    services.AddTransient<ITestSwitchableService, TestSwitchableKeep>();
        //    services.AddTransient<ITestSwitchableService, TestSwitchableSkip>();
        //    services
        //        .AddLibCore();
        //}

        protected override void SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
            services.AddTransient<ITestSwitchableService, TestSwitchableFallback>();
            services.AddTransient<ITestSwitchableService, TestSwitchableKeep>();
            services.AddTransient<ITestSwitchableService, TestSwitchableSkip>();
            services
                .AddLibCore();
        }

    }
}
