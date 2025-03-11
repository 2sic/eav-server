using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.ConfigurationOverride;

/// <summary>
/// Special scenario which has a configuration to override the Fancybox3 Web Resources
/// </summary>
public record ScenarioOverrideFancybox3 : ScenarioBasic
{
    public override string GlobalDataCustomFolder => TestFiles.GetTestPath("ScenarioData\\OverrideFancybox3") + $"\\{ScenarioConstants.DevMaterialsEnd}";
}