using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    public class PersistenceFile: HasLog //: Efc11TestBase
    {
        public TestContext TestContext { get; set; }

        private const string OrigPath = "Persistence.File\\.data\\";
        private const string TestingPath = "testApp";
        public PersistenceFile() : base("Tst.FSLoad") { }


        [TestMethod]
        [DeploymentItem("..\\..\\" + OrigPath, TestingPath)]
        public void Json_LoadFile_SqlDataSource()
        {
            var root = TestContext.DeploymentDirectory + "\\" + TestingPath + "\\";
            var loader = new FileSystemLoader(root, false, Log);
            IList<IContentType> cts = new List<IContentType>();
            try
            {
                cts = loader.ContentTypes(0, null);
            }
            finally 
            {
                Trace.Write(Log.Dump());
            }


            Assert.AreEqual(3, cts.Count, "test case has 3 content-types to deserialize");
            
        }
        
    }
}
