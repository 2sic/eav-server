using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.ExternalData
{
    [TestClass]
    public class DataTableTst: TestBaseEavDataSource
    {

        [TestMethod]
        public void DataSource_Create_GeneralTest()
        {
            const int itemsToGenerate = 499;
            var ds = new DataTablePerson(this).Generate(itemsToGenerate);
            Assert.IsTrue(ds.InForTests().Count == 0, "In count should be 0");
            Assert.IsTrue(ds.Out.Count == 1, "Out count should be 1");
            var defaultOut = ds[DataSourceConstants.StreamDefaultName];
            Assert.IsTrue(defaultOut != null);
            try
            {
                // ReSharper disable once UnusedVariable
                var x = ds["Something"];
                Assert.Fail("Access to another out should fail");
            }
            catch { }
            Assert.IsTrue(defaultOut.ListForTests().Count() == itemsToGenerate);
        }

        [TestMethod]
        public void DataTable_CacheKey()
        {
            const int itemsToGenerate = 499;
            var ds = new DataTablePerson(this).Generate(itemsToGenerate);

            //var expKey = "DataTable:NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person";
            var expKey = "DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName";
            Assert.AreEqual(expKey, ds.CachePartialKey);
            Assert.AreEqual(expKey, ds.CacheFullKey);
            var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
            Assert.IsTrue(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");
        }

        [TestMethod]
        public void DataTable_DefaultTitleField()
        {
            const int itemsToGenerate = 25;
            var ds = new DataTableTrivial(this).Generate(itemsToGenerate);

            Assert.AreEqual(25, ds.ListForTests().Count());
            var first = ds.ListForTests().FirstOrDefault();
            Assert.AreEqual("Daniel Mettler", first.GetBestTitle());
        }


    }
}
