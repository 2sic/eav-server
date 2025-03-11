namespace ToSic.Eav.Testing.Scenarios;

/// <summary>
/// Basic setup with all the defaults and no special features / licenses activated.
/// </summary>
public record ScenarioBasic: TestScenario
{
    public ScenarioBasic()
    {
        ConStr = ScenarioConstants.DefaultConnectionString;
        GlobalFolder = ScenarioConstants.DefaultGlobalFolder;
        GlobalDataCustomFolder = $"{ScenarioConstants.DevMaterialsRoot}ScenarioBasic\\{ScenarioConstants.DevMaterialsEnd}";
    }
}