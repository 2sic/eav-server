using Microsoft.Extensions.DependencyInjection;
using ToSic.Mocks.Named;
using ToSic.Sys.DI;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Services.Generator.Keyed;

public class KeyedGenerator(Generator<IMockNamedService> generator)
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) =>
            services.AddMockNamedServices().AddSysCoreDi();
    }

    [Fact]
    public void GetAbcWorks()
        => Equal(MockNamedServiceAbc.NameIdConst, generator.New(MockNamedServiceAbc.NameIdConst).NameId);
}
