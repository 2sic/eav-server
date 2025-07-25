﻿namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

/// <summary>
/// Basic setup with all the defaults and no special features / licenses activated.
/// </summary>
public record ScenarioMini: TestScenario
{
    public override string ConStr => ScenarioConstants.DefaultConnectionString;
    public override string GlobalFolder => TestFiles.GetTestPath(PersistenceTestConstants.ScenarioMini);
    public override string GlobalDataCustomFolder => ""; // $"{ScenarioConstants.DevMaterialsRoot}ScenarioBasic\\{ScenarioConstants.DevMaterialsEnd}";
    //public override string AppsShared => ScenarioConstants.DefaultGlobalFolder;

    public const int Has3Queries = 3;
}