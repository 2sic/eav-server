using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests.Types;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File
{
    [TestClass]
    public class RuntimeLoaderTest:PersistenceTestsBase
    {
        public RuntimeLoaderTest()
        {
            _appLoader = GetService<IAppLoader>();
            _globalAppState = GetService<IAppReaders>().GetSystemPreset();
        }
        private readonly IAppLoader _appLoader;
        private readonly IAppReader _globalAppState;

        private int expectedTypesSysAndJson = 5;
        [Ignore("currently work in progress - as sys/json types keep changing and testing isn't updated yet")]
        [TestMethod]
        [DeploymentItem("..\\..\\" + PathWith3Types, TestingPath3)]
        public void TestWith3FileTypes()
        {
            // set loader root path, based on test environment
            TestGlobalFolderRepository.PathToUse = TestStorageRoot;

            var all = _globalAppState.ContentTypes;
            Assert.AreEqual(expectedTypesSysAndJson, all.Count());

            var hasCodeSql = all.FirstOrDefault(t => t.Name.Contains("SqlData"));
            Assert.IsNotNull(hasCodeSql, "should find code sql");

            Assert.IsTrue(hasCodeSql is TypesBase, "sql should come from code, and not from json, as code has higher priority");

            var whateverType = all.FirstOrDefault(t => t.Name == "Whatever");
            Assert.IsNotNull(whateverType, "should find whatever type from json");
            //var dummy = all.First();
            //Assert.AreEqual(DemoType.CTypeName, dummy.Key);
        }

        [TestMethod]
        [DeploymentItem("..\\..\\" + PathWith40Types, TestingPath40)]
        public void TestWith40FileTypes_JustReRunIfItFails()
        {
            // set loader root path, based on test environment

            var time = Stopwatch.StartNew();
            TestGlobalFolderRepository.PathToUse = TestingPath40;
            var count = _globalAppState.ContentTypes.Count();
            time.Stop();
            
            Assert.IsTrue(count >= 80 && count <= 150, $"expected between 75 and 140, actually is {count}");
            Trace.WriteLine("time used: " + time.Elapsed);
        }

        [TestMethod]
        [DeploymentItem("..\\..\\" + PathWith40Types, TestingPath40)]
        public void TestConcurrentInitialize()
        {
            int res1 = 0, res2 = 0;
            TestGlobalFolderRepository.PathToUse = TestingPath40;

            var t1 = new Thread(() => res1 = _globalAppState.ContentTypes.Count());
            var t2 = new Thread(() => res2 = _globalAppState.ContentTypes.Count());

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.IsTrue(res1 > 0, "should have gotten a count");
            Assert.AreEqual(res1, res2, "both res should be equal");
        }

        [TestMethod]
        [DeploymentItem("..\\..\\" + PathWith40Types, TestingPath40)]
        public void TimingWith400FileTypes()
        {
            // set loader root path, based on test environment
            TestGlobalFolderRepository.PathToUse = TestingPath40;
            var loader = (AppLoader)_appLoader;
            var time = Stopwatch.StartNew();
            for (var i = 0; i < 10; i++)
            {
                loader.Loaders.First().ContentTypes();
                Trace.WriteLine($"time after cycle {i} was {time.Elapsed}");
            }
            time.Stop();

            Trace.WriteLine("time used to load 400 with debug/testing overhead: " + time.Elapsed);
        }

    }
}
