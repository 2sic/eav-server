using Microsoft.Extensions.DependencyInjection;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests
{
    public class VerifySwitchableServiceBase : TestBaseDiRaw
    {
        protected override IServiceCollection SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
            services.AddTransient<ITestSwitchableService, TestSwitchableFallback>();
            services.AddTransient<ITestSwitchableService, TestSwitchableKeep>();
            services.AddTransient<ITestSwitchableService, TestSwitchableSkip>();
            services.AddEavCorePlumbing();
            return services;
        }
        
    }
}
