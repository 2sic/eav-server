using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File
{
    [TestClass]
    public class RuntimeLoaderTest:PersistenceTestsBase
    {
        private int expectedTypesSysAndJson = 5;
        [Ignore("currently work in progress - as sys/json types keep changing and testing isn't updated yet")]
        [TestMethod]
        [DeploymentItem("..\\..\\" + PathWith3Types, TestingPath3)]
        public void TestWith3FileTypes()
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

        [TestMethod]
        [DeploymentItem("..\\..\\" + PathWith40Types, TestingPath40)]
        public void TestWith40FileTypes()
        {
            // set loader root path, based on test environment
            DemoRuntime.PathToUse = TestingPath40;

            var time = Stopwatch.StartNew();
            var all = Global.AllContentTypes();
            time.Stop();
            
            Assert.AreEqual(42, all.Count);
            Trace.WriteLine("time used: " + time.Elapsed);
        }

        [TestMethod]
        public void TestWith400FileTypes()
        {
            // set loader root path, based on test environment
            DemoRuntime.PathToUse = TestingPath40;
            var loader = new DemoRuntime();
            var time = Stopwatch.StartNew();
            for (var i = 0; i < 10; i++)
            {
                loader.Loader.ContentTypes(0, null);
                Trace.WriteLine($"time after cycle {i} was {time.Elapsed}");
            }
            time.Stop();

            Trace.WriteLine("time used to load 400 with debug/testing overhead: " + time.Elapsed);
        }

    }
}
