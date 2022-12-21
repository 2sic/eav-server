using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    public class Tst_FileLoader_Ct: PersistenceTestsBase
    {

        [TestMethod]
        public void FLoader_LoadAllAndCount()
        {
            var cts = LoadAllTypes();
            Assert.AreEqual(3, cts.Count, "test case has 3 content-types to deserialize");
            var sqlType = cts.FirstOrDefault(ct => ct.NameId.Contains("Sql"));
            Assert.IsNotNull(sqlType, "should find the sql type");
            Assert.AreEqual("System", sqlType.Scope, "scope should be system");
        }

        [TestMethod]
        public void FLoader_SqlType()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.FirstOrDefault(ct => ct.NameId.Contains("Sql"));
            Assert.IsNotNull(sqlType, "should find the sql type");
            Assert.AreEqual("System", sqlType.Scope, "scope should be system");
        }

        [TestMethod]
        public void FLoader_CheckDynamicAttributes()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.First(ct => ct.NameId.Contains("Sql"));
            Assert.AreEqual(9, sqlType.Attributes.Count, "sql type should have x attributes");

            var conStrName = "ConnectionString";
            var conStr = sqlType.Attributes.FirstOrDefault(a => a.Name == conStrName);
            Assert.IsNotNull(conStr, $"should find the {conStrName} field");

            var title = sqlType.Attributes.FirstOrDefault(a => a.IsTitle);
            Assert.IsNotNull(title, "should find title field");

            var conMeta = conStr.Metadata;
            Assert.AreEqual(2, conMeta.Count(), "constr should have 2 meta-items");

            var conMetaAll = conMeta.FirstOrDefault(e => e.Type.Name == AttributeMetadata.TypeGeneral);
            Assert.IsNotNull(conMetaAll, "should have @all metadata");

            var conMetaStr = conMeta.FirstOrDefault(e => e.Type.Name == "@string-default");
            Assert.IsNotNull(conMetaStr, "should have string metadata");

            var lines = (decimal)conMetaStr.Value("RowCount");
            Assert.AreEqual(3, lines);
        }



        [TestMethod]
        public void FLoader_CheckTypeMetadata()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.First(ct => ct.NameId.Contains("Sql"));
            Assert.AreEqual(9, sqlType.Attributes.Count, "sql type should have x attributes");
            

            var meta = sqlType.Metadata;
            Assert.AreEqual(2, meta.Count(), "should have 2 meta-items");

            var conMetaAll = meta.FirstOrDefault(e => e.Type.Name == "Basics");
            Assert.IsNotNull(conMetaAll, "should have Basics metadata");

            var niceName = conMetaAll.Value<string>("NiceName");
            Assert.AreEqual("sql content type", niceName);
            var active = conMetaAll.Value<bool>("Active");
            Assert.IsTrue(active, "should have an active-info");


            var conMetaStr = meta.FirstOrDefault(e => e.Type.Name == "Enhancements");
            Assert.IsNotNull(conMetaStr, "should have Enhancements metadata");

            var icon = conMetaStr.Value<string>("Icon");
            Assert.AreEqual("icon.png", icon);
        }






        private IList<IContentType> LoadAllTypes()
        {
            var loader = Build<FileSystemLoader>()
                .Init(Constants.PresetAppId, TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
            IList<IContentType> cts;
            try
            {
                cts = loader.ContentTypes();
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return cts;
        }

        
    }
}
