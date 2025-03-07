using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Tests.Persistence.File;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    public class Tst_FileLoader_Ct: FileLoaderCtBase
    {

        [TestMethod]
        public void FLoader_LoadAllAndCount()
        {
            var expected = 8;
            var cts = LoadAllTypes();
            AreEqual(8, cts.Count, $"test case has {expected} content-types to deserialize, found {cts.Count}");
        }

        [TestMethod]
        public void FLoader_SqlType()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.FirstOrDefault(ct => ct.NameId.Contains("Sql"));
            IsNotNull(sqlType, "should find the sql type");
            AreEqual("System", sqlType.Scope, "scope should be system");
        }

        [TestMethod]
        public void FLoader_CheckDynamicAttributes()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.First(ct => ct.NameId.Contains("Sql"));
            AreEqual(9, sqlType.Attributes.Count(), "sql type should have x attributes");

            var conStrName = "ConnectionString";
            var conStr = sqlType.Attributes.FirstOrDefault(a => a.Name == conStrName);
            IsNotNull(conStr, $"should find the {conStrName} field");

            var title = sqlType.Attributes.FirstOrDefault(a => a.IsTitle);
            IsNotNull(title, "should find title field");

            var conMeta = conStr.Metadata;
            AreEqual(2, conMeta.Count(), "constr should have 2 meta-items");

            var conMetaAll = conMeta.FirstOrDefault(e => e.Type.Name == AttributeMetadata.TypeGeneral);
            IsNotNull(conMetaAll, "should have @all metadata");

            var conMetaStr = conMeta.FirstOrDefault(e => e.Type.Name == "@string-default");
            IsNotNull(conMetaStr, "should have string metadata");

            var lines = (decimal)conMetaStr.GetTac("RowCount");
            AreEqual(3, lines);
        }



        [TestMethod]
        public void FLoader_CheckTypeMetadata()
        {
            var cts = LoadAllTypes();
            var sqlType = cts.First(ct => ct.NameId.Contains("Sql"));
            AreEqual(9, sqlType.Attributes.Count(), "sql type should have x attributes");
            

            var meta = sqlType.Metadata;
            AreEqual(2, meta.Count(), "should have 2 meta-items");

            var conMetaAll = meta.FirstOrDefault(e => e.Type.Name == "Basics");
            IsNotNull(conMetaAll, "should have Basics metadata");

            var niceName = conMetaAll.GetTac<string>("NiceName");
            AreEqual("sql content type", niceName);
            var active = conMetaAll.GetTac<bool>("Active");
            IsTrue(active, "should have an active-info");


            var conMetaStr = meta.FirstOrDefault(e => e.Type.Name == "Enhancements");
            IsNotNull(conMetaStr, "should have Enhancements metadata");

            var icon = conMetaStr.GetTac<string>("Icon");
            AreEqual("icon.png", icon);
        }


    }
}
