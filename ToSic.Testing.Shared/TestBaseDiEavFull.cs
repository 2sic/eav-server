using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Base class for tests providing all the Eav dependencies (Apps, etc.)
    /// </summary>
    public abstract class TestBaseDiEavFull: TestBaseDiEmpty
    {
        protected override IServiceCollection SetupServices(IServiceCollection services)
        {
            return base.SetupServices(services)
                .AddEav();
        }
    }
}
