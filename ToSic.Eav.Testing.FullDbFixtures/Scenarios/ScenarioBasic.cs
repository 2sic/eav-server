namespace ToSic.Eav.Testing.Scenarios;

/// <summary>
/// Basic setup with all the defaults and no special features / licenses activated.
/// </summary>
public record ScenarioBasic : TestScenario
{
    public override string ConStr => ScenarioConstants.DefaultConnectionString;
    public override string GlobalFolder => ScenarioConstants.DefaultGlobalFolder;

    public override string GlobalDataCustomFolder => $"{ScenarioConstants.DevMaterialsRoot}ScenarioBasic\\{ScenarioConstants.DevMaterialsEnd}";

    public override string AppsShared => ScenarioConstants.DefaultGlobalFolder;

    //public override string AppsSite => ScenarioConstants.TestAppsSite01Root;
}