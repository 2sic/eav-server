using ToSic.Eav.Persistence.Efc.Tests;
using ToSic.Testing.Shared;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

[DeploymentItem("..\\..\\" + PathWith3Types, TestingPath3)]
public class PersistenceTestsBase : Efc11TestBase
{
    public TestContext TestContext { get; set; }

    public const string PathWith3Types = "Persistence.File\\.data\\";
    public const string PathWith40Types = "Persistence.File\\all-for-dnn\\";
    public const string TestingPath3 = "testApp";
    public const string TestingPath40 = "all-for-dnn";
    public const string ExportPath = "exp";

    //public PersistenceTestsBase() : base("Tst.FSLoad") { }


    public string TestStorageRoot => TestContext.DeploymentDirectory + "\\" + TestingPath3 + "\\";

    public string ExportStorageRoot => TestContext.DeploymentDirectory + "\\" + ExportPath + "\\";

}