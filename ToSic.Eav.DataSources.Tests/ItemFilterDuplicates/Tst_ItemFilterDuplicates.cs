using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.UnitTests.DataSources;



namespace ToSic.Eav.DataSources.Tests.ItemFilterDuplicates
{
    [TestClass]
    public class Tst_ItemFilterDuplicates
    {
        [TestMethod]
        public void ItemFilterDuplicates_In0()
        {
            var desiredFinds = 0;
            var sf = new DataSource(null).GetDataSource<DataSources.ItemFilterDuplicates>(
                new AppIdentity(0,0), null, 
                configLookUp: new LookUpEngine(null as ILog));
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
            var sf = new DataSource(null).GetDataSource<DataSources.StreamMerge>(new AppIdentity(0, 0), ds);

            for (int i = 1; i < attach; i++)
                sf.In.Add("another" + i, ds.Out.First().Value);

            var unique = new DataSource(null).GetDataSource<DataSources.ItemFilterDuplicates>(new AppIdentity(0, 0), sf);
            return unique;
        }
    }
}
