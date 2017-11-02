using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Logging;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File
{
    [DeploymentItem("..\\..\\" + OrigPath, TestingPath)]
    public class PersistenceTestsBase : HasLog
    {
        public TestContext TestContext { get; set; }

        public const string OrigPath = "Persistence.File\\.data\\";
        public const string TestingPath = "testApp";
        public const string ExportPath = "exp";

        public PersistenceTestsBase() : base("Tst.FSLoad") { }


        public string TestStorageRoot => TestContext.DeploymentDirectory + "\\" + TestingPath + "\\";

        public string ExportStorageRoot => TestContext.DeploymentDirectory + "\\" + ExportPath + "\\";

    }
}
