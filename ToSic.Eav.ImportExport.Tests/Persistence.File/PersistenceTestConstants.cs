namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

class PersistenceTestConstants
{
    public const string PathWith3Types = "Persistence.File\\.data\\";
    public const string PathWith40Types = "Persistence.File\\all-for-dnn\\";
    public const string TestingPath3 = "testApp";
    public const string TestingPath40 = "all-for-dnn";
    public const string ExportPath = "exp";

    public static string ExportStorageRoot(TestContext testContext) =>
        $"{testContext.DeploymentDirectory}\\{ExportPath}\\";

    public static string TestStorageRoot(TestContext testContext) =>
        testContext.DeploymentDirectory + "\\" + TestingPath3 + "\\";

}