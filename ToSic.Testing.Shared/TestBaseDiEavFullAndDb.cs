using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav;
using ToSic.Eav.Configuration;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Base class for tests providing all the Eav dependencies (Apps, etc.)
    /// </summary>
    public abstract class TestBaseDiEavFullAndDb: TestBaseDiEmpty
    {
        protected TestBaseDiEavFullAndDb()
        {
            // this will run after the base contructor, which configures DI
            Build<IDbConfiguration>().ConnectionString = DbConnectionString;
        }

        protected virtual string DbConnectionString => TestConstants.ConStr;

        protected override IServiceCollection SetupServices(IServiceCollection services = null)
        {
            return base.SetupServices(services)
                .AddEav();
        }
    }
}
