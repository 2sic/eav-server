﻿namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

class PersistenceTestConstants
{
    public const string ScenarioRoot = "Persistence.File\\Scenarios\\";


    public const string PathMini = "Mini\\";// "Persistence.File\\.data\\";
    public const string ScenarioMini = ScenarioRoot + "Mini\\";
    public const string ScenarioMiniDeep = ScenarioMini + "App_Data\\system";
    //public const string PathWith40Types = "all-for-dnn\\"; // "Persistence.File\\all-for-dnn\\";
    public const string Scenario40Types = ScenarioRoot + "40-types\\";
    public const string TestingPath3 = "testApp";
    public const string TestingPath40 = "all-for-dnn";
    public const string ExportPath = "exp";

    //public static string ExportStorageRoot(TestContext testContext) =>
    //    $"{testContext.DeploymentDirectory}\\{ExportPath}\\";

    //public static string TestStorageRoot(TestContext testContext) =>
    //    testContext.DeploymentDirectory + "\\" + TestingPath3 + "\\";

}