﻿namespace ToSic.Eav.ImportExport.Tests.Persistence.File.RuntimeLoader;

/// <summary>
/// Basic setup with all the defaults and no special features / licenses activated.
/// </summary>
public record Scenario40TypesInSingleFiles : TestScenario
{
    public override string ConStr => ScenarioConstants.DefaultConnectionString;
    public override string GlobalFolder => TestFiles.GetTestPath(PersistenceTestConstants.Scenario40Types);
    public override string GlobalDataCustomFolder => ""; // $"{ScenarioConstants.DevMaterialsRoot}ScenarioBasic\\{ScenarioConstants.DevMaterialsEnd}";
}