using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.DI;
using ToSic.Sys.Boot;

namespace ToSic.Eav.Testing;

public class DoFixtureStartup<TScenario> where TScenario : TestScenario, new()
{
    public DoFixtureStartup(LazySvc<FixtureStartupNoDb> fixtureStartupBasic, LazySvc<FixtureStartupWithDb> fixtureSetupDb, BootCoordinator bootCoordinator)
    {
        var testScenario = new TScenario();
        if (testScenario.UseDb)
            fixtureSetupDb.Value.SetupFixtureConfiguration(testScenario);
        else
            fixtureStartupBasic.Value.SetupFixtureConfiguration(testScenario);

        bootCoordinator.StartUp();
    }
}