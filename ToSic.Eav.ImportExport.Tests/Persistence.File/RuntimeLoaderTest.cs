using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File
{
    [TestClass]
    
    public class RuntimeLoaderTest:PersistenceTestsBase
    {
        private int expectedTypesSysAndJson = 5;
        [TestMethod]
        public void GlobalsContainsExpectedTypes()
        {
            // set loader root path, based on test environment
            DemoRuntime.PathToUse = TestStorageRoot;

            var all = Global.AllContentTypes();
            Assert.AreEqual(expectedTypesSysAndJson, all.Count);

            var hasCodeSql = all.Values.FirstOrDefault(t => t.Name.Contains("SqlData"));
            Assert.IsNotNull(hasCodeSql, "should find code sql");

            Assert.IsTrue(hasCodeSql is TypesBase, "sql should come from code, and not from json, as code has higher priority");

            var whateverType = all.Values.FirstOrDefault(t => t.Name == "Whatever");
            Assert.IsNotNull(whateverType, "should find whatever type from json");
            //var dummy = all.First();
            //Assert.AreEqual(DemoType.CTypeName, dummy.Key);

        }
    }
}
