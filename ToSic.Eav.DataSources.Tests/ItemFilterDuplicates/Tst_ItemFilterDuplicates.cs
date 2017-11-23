using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.UnitTests.DataSources;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.DataSources.Tests.ItemFilterDuplicates
{
    [TestClass]
    public class Tst_ItemFilterDuplicates
    {
        [TestMethod]
        public void ItemFilterDuplicates_In0()
        {
            var desiredFinds = 0;
            var sf = DataSource.GetDataSource<DataSources.ItemFilterDuplicates>(0, 0, valueCollectionProvider: new ValueCollectionProvider());
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].List.Count();
            Assert.AreEqual(desiredFinds, dupls, "Should find exactly this amount people");
        }

        [TestMethod]
        public void StreamMerge_In1()
        {
            var desiredFinds = 100;
            var desiredDupls = 0;
            var sf = GenerateDuplsDs(desiredFinds, 1);
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].List.Count();
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

            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].List.Count();
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

            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

            var dupls = sf[DataSources.ItemFilterDuplicates.DuplicatesStreamName].List.Count();
            Assert.AreEqual(desiredDupls, dupls, "Should find exactly this amount people");
        }



        private static DataSources.ItemFilterDuplicates GenerateDuplsDs(int desiredFinds, int attach)
        {
            if(attach < 1) throw new Exception("attach must be at least 1");
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(desiredFinds, 1001, true);
            var sf = DataSource.GetDataSource<DataSources.StreamMerge>(0, 0, ds);

            for (int i = 1; i < attach; i++)
                sf.In.Add("another" + i, ds.Out.First().Value);

            var unique = DataSource.GetDataSource<DataSources.ItemFilterDuplicates>(0, 0, sf);
            return unique;
        }
    }
}
