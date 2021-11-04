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
            // this will run after the base constructor, which configures DI
            var dbConfiguration = Build<IDbConfiguration>();
            dbConfiguration.ConnectionString = DbConnectionString;

            var globalConfig = Build<IGlobalConfiguration>();
            globalConfig.DataFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";

            // Make sure global types are loaded
            Build<SystemLoader>().StartUp();
        }

        protected virtual string DbConnectionString => TestConstants.ConStr;

        protected override IServiceCollection SetupServices(IServiceCollection services = null)
        {
            return base.SetupServices(services)
                .AddEav();
        }
    }
}
