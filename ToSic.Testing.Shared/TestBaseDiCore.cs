using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Base class for tests using very basic dependencies only
    /// </summary>
    public abstract class TestBaseDiCore: TestBaseDiEmpty
    {
        protected override IServiceCollection SetupServices(IServiceCollection services)
        {
            return base.SetupServices(services)
                .AddEavCore()
                .AddEavCorePlumbing()
                .AddEavCoreFallbackServices();
        }
    }
}
