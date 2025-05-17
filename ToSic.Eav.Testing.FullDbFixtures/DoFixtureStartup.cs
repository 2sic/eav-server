using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.DI;

namespace ToSic.Eav.Testing;

public class DoFixtureStartup<TScenario> where TScenario : TestScenario, new()
{
    public DoFixtureStartup(LazySvc<FixtureStartupNoDb> fixtureStartupBasic, LazySvc<FixtureStartupWithDb> fixtureSetupDb, SystemLoader systemLoader)
    {
        var testScenario = new TScenario();
        if (testScenario.UseDb)
            fixtureSetupDb.Value.SetupFixtureConfiguration(testScenario);
        else
            fixtureStartupBasic.Value.SetupFixtureConfiguration(testScenario);

        systemLoader.StartUp();
    }
}