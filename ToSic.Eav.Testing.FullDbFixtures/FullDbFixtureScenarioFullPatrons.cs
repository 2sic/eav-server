using ToSic.Eav.Internal.Loaders;

namespace ToSic.Eav.Testing;

public class FullDbFixtureScenarioFullPatrons
{
    public FullDbFixtureScenarioFullPatrons(FullDbFixtureHelper fixtureHelper, SystemLoader systemLoader)
    {
        fixtureHelper.Configure(EavTestConfig.ScenarioFullPatrons);
        systemLoader.StartUp();
    }
}