using ToSic.Eav.Internal.Loaders;
using ToSic.Testing;

namespace ToSic.Eav.DataSource.DbTests;

public class FullDbFixture
{
    public FullDbFixture(FullDbFixtureHelper fixtureHelper, SystemLoader systemLoader)
    {
        fixtureHelper.Configure(EavTestConfig.ScenarioBasic);
        systemLoader.StartUp();
    }
}