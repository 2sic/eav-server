using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.Testing;

public class DoFixtureStartup<TScenario> where TScenario : TestScenario, new()
{
    public DoFixtureStartup(FullDbFixtureHelper fixtureHelper, SystemLoader systemLoader)
    {
        fixtureHelper.Configure(new TScenario());
        systemLoader.StartUp();
    }
}