using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Base class for tests providing all the Eav dependencies (Apps, etc.)
    /// </summary>
    public abstract class TestBaseDiEavFull: TestBaseDiEmpty
    {
        //protected TestBaseDiEavFull()
        //{
        //    ServiceCollection.AddEav();
        //}

        protected override void SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
            services
                .AddEav();
        }
    }
}
