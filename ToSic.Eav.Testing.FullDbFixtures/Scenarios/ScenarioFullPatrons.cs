namespace ToSic.Eav.Testing.Scenarios;

/// <summary>
/// Advanced Patrons setup with additional licenses activated etc.
/// Requires that the developer has access to the dev-materials repository.
/// </summary>
public record ScenarioFullPatrons : TestScenario
{
    public ScenarioFullPatrons()
    {
        ConStr = ScenarioConstants.DefaultConnectionString;
        GlobalFolder = ScenarioConstants.DefaultGlobalFolder;
        GlobalDataCustomFolder = ScenarioConstants.DevMaterialsRoot + ScenarioConstants.DevMaterialsEnd;
    }
}