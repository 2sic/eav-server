namespace ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;

/// <summary>
/// Basic setup with all the defaults and no special features / licenses activated.
/// </summary>
public record ScenarioDotData: TestScenario
{
    public override string ConStr => ScenarioConstants.DefaultConnectionString;
    public override string GlobalFolder => TestFiles.GetTestPath(PersistenceTestConstants.ScenarioDotData);
    public override string GlobalDataCustomFolder => ""; // $"{ScenarioConstants.DevMaterialsRoot}ScenarioBasic\\{ScenarioConstants.DevMaterialsEnd}";
}