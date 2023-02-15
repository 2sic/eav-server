using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    [TestClass]
    public class ItemFilterDuplicatesTest: TestBaseDiEavFullAndDb
    {
        [TestMethod]
        public void ItemFilterDuplicates_In0()
        {
            var desiredFinds = 0;
            var sf = this.GetTestDataSource<DataSources.ItemFilterDuplicates>();
            var found = sf.ListForTests().Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].ListForTests().Count();
            Assert.AreEqual(desiredFinds, dupls, "Should find exactly this amount people");
        }

        [TestMethod]
        public void StreamMerge_In1()
        {
            var desiredFinds = 100;
            var desiredDupls = 0;
            var sf = GenerateDuplsDs(desiredFinds, 1);
            var found = sf.ListForTests().Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].ListForTests().Count();
            Assert.AreEqual(desiredDupls, dupls, "Should find exactly this amount people");
        }

        [TestMethod]
        public void StreamMerge_In2()
        {
            // fi
            var items = 100;
            var desiredFinds = items;
            var desiredDupls = items;
            var sf = GenerateDuplsDs(items, 2);

            var found = sf.ListForTests().Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].ListForTests().Count();
            Assert.AreEqual(desiredDupls, dupls, "Should find exactly this amount people");
        }

         [TestMethod]
        public void StreamMerge_In3()
        {
            // fi
            var items = 100;
            var desiredFinds = items;
            var desiredDupls = items;
            var sf = GenerateDuplsDs(items, 3);

            var found = sf.ListForTests().Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].ListForTests().Count();
            Assert.AreEqual(desiredDupls, dupls, "Should find exactly this amount people");
        }



        private DataSources.ItemFilterDuplicates GenerateDuplsDs(int desiredFinds, int attach)
        {
            if(attach < 1) throw new Exception("attach must be at least 1");
            var ds = new DataTablePerson(this).Generate(desiredFinds, 1001, true);
            var dsf = GetService<DataSourceFactory>();
            var sf = dsf.GetDataSource<StreamMerge>(new AppIdentity(0, 0), ds);

            for (int i = 1; i < attach; i++)
                sf.InForTests().Add("another" + i, ds.Out.First().Value);

            var unique = dsf.GetDataSource<DataSources.ItemFilterDuplicates>(new AppIdentity(0, 0), sf);
            return unique;
        }
    }
}
