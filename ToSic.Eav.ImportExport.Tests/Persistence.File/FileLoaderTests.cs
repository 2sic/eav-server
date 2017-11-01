using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\" + OrigPath, TestingPath)]
    public class FileLoaderTests: HasLog //: Efc11TestBase
    {
        public TestContext TestContext { get; set; }

        private const string OrigPath = "Persistence.File\\.data\\";
        private const string TestingPath = "testApp";
        public FileLoaderTests() : base("Tst.FSLoad") { }


        [TestMethod]
        public void FLoader_LoadAllAndCount()
        {
            var cts = LoadAllTypes();
            Assert.AreEqual(3, cts.Count, "test case has 3 content-types to deserialize");
            var sqlType = cts.FirstOrDefault(ct => ct.StaticName.Contains("Sql"));
            Assert.IsNotNull(sqlType, "should find the sql type");
            Assert.AreEqual("System", sqlType.Scope, "scope should be system");
        }

        [TestMethod]
        public void FLoader_SqlType()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.FirstOrDefault(ct => ct.StaticName.Contains("Sql"));
            Assert.IsNotNull(sqlType, "should find the sql type");
            Assert.AreEqual("System", sqlType.Scope, "scope should be system");
        }

        [TestMethod]
        public void FLoader_CheckDynamicAttributes()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.First(ct => ct.StaticName.Contains("Sql"));
            Assert.AreEqual(9, sqlType.Attributes.Count, "sql type should have x attributes");

            var conStrName = "ConnectionString";
            var conStr = sqlType.Attributes.FirstOrDefault(a => a.Name == conStrName);
            Assert.IsNotNull(conStr, $"should find the {conStrName} field");

            var title = sqlType.Attributes.FirstOrDefault(a => a.IsTitle);
            Assert.IsNotNull(title, "should find title field");

            var conMeta = conStr.MetadataItems;
            Assert.AreEqual(2, conMeta.Count, "constr should have 2 meta-items");

            var conMetaAll = conMeta.FirstOrDefault(e => e.Type.Name == "@All");
            Assert.IsNotNull(conMetaAll, "should have @all metadata");

            var conMetaStr = conMeta.FirstOrDefault(e => e.Type.Name == "@string-default");
            Assert.IsNotNull(conMetaStr, "should have string metadata");

            // todo: test values in the conmetastr
            var lines = (decimal)conMetaStr.GetBestValue("RowCount");
            Assert.AreEqual(3, lines);
        }

        private IList<IContentType> LoadAllTypes()
        {
            var root = TestContext.DeploymentDirectory + "\\" + TestingPath + "\\";
            var loader = new FileSystemLoader(root, false, Log);
            IList<IContentType> cts;
            try
            {
                cts = loader.ContentTypes(0, null);
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return cts;
        }
    }
}
