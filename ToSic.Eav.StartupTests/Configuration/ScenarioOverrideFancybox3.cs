using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.Configuration;

/// <summary>
/// Basic setup with all the defaults and no special features / licenses activated.
/// </summary>
public record ScenarioOverrideFancybox3 : ScenarioBasic
{
    public override string GlobalDataCustomFolder => $"{ScenarioConstants.DevMaterialsRoot}ScenarioOverrideFancybox3\\{ScenarioConstants.DevMaterialsEnd}";
}