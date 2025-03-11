using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Testing.Shared;

/// <summary>
/// Base class for tests providing all the Eav dependencies (Apps, etc.)
/// </summary>
public abstract class TestBaseDiEavFullAndDb(TestScenario testScenario = default) : TestBaseEav(testScenario)
{
    protected override void Configure()
    {
        base.Configure();

        // Make sure global data is loaded
        GetService<SystemLoader>().StartUp();
    }

}