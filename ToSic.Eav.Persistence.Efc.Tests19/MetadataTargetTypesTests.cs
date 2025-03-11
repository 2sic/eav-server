using ToSic.Eav.Metadata;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.Persistence.Efc.Tests19;


[Startup(typeof(StartupTestFullWithDb))]
public class MetadataTargetTypesTests(ITargetTypes targetTypes) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void HasExactly100TargetTypes() =>
        Equal(100, targetTypes.TargetTypes.Count);

    [Fact]
    public void FirstTargetTypeIsDefault() =>
        Equal("Default", targetTypes.TargetTypes[(int)TargetTypes.None]);
}