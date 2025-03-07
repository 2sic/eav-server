using ToSic.Eav.Internal.Loaders;

namespace ToSic.Eav.Testing;

public class FullDbFixtureScenarioBasic
{
    public FullDbFixtureScenarioBasic(FullDbFixtureHelper fixtureHelper, SystemLoader systemLoader)
    {
        fixtureHelper.Configure(EavTestConfig.ScenarioBasic);
        systemLoader.StartUp();
    }
}