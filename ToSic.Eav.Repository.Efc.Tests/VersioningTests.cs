using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Versioning;
using ToSic.Eav.Repository.Efc.Tests.Mocks;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class VersioningTests
    {
        #region Test Data

        private int TestItemWithCa20Changes = 3893;
        private Guid ItemWithCa20Changes = new Guid("5BF76130-B386-4848-9F17-AAE567D50CF6");
        private int TestItemWithCa100Changes = 4457;
        private Guid ItemWithCa100Changes = new Guid("23F24A0C-B4A2-43B4-ACBF-F5AC64549BF1");
        private int TestItemWithCa9Changes = 7707;
        private Guid ItemWithCa9Changes = new Guid("3AB334D9-A99C-4C01-AAB7-C29E04274B5C");

        private int DevPc2dmItemOnHome = 3269;
        private int DevPc2dmAppId = 2;
        private int DevPc2dmZoneId = 2;
        private int DevPc2dmLanguageDefault = 37;

        // just a note: this is Portal 54 or something on Daniels test server, change it to your system if you want to run these tests!
        public const int ZoneId = 56;

        public IImportExportEnvironment Environment = new ImportExportEnvironmentMock();


        #endregion

        [TestMethod]
        public void DevPc2dmRestoreV18()
        {
            var id = DevPc2dmItemOnHome;
            var version = 17;
            var dc = DbDataController.Instance(DevPc2dmZoneId, DevPc2dmAppId);
            var all = dc.Versioning.GetHistoryList(id, false);
            var vId = all.First(x => x.VersionNumber == version).ChangeSetId;
            dc.Versioning.RestoreEntity(DevPc2dmItemOnHome, vId);

        }

        [TestMethod]
        public void GetHistoryTests()
        {
            GetHistoryTest(TestItemWithCa20Changes, 20);
            GetHistoryTest(TestItemWithCa100Changes, 102);
            GetHistoryTest(TestItemWithCa9Changes, 9);
        }

        [TestMethod]
        public void GetVersion6OfItemWith20()
        {
            var id = TestItemWithCa20Changes;
            var version = 6;
            var all = DbDataController.Instance(ZoneId).Versioning.GetHistoryList(id, false);
            var vId = all.First(x => x.VersionNumber == version).ChangeSetId;
            var vItem = DbDataController.Instance(ZoneId).Versioning.GetItem(id, vId);
            Console.Write(vItem.Data);
        }


        

        private List<ItemHistory> GetHistoryTest(int entityId, int expectedCount)
        {
            var history = DbDataController.Instance(ZoneId).Versioning.GetHistoryList(entityId, true);
            Assert.AreEqual(expectedCount, history.Count, "should have 20 items in history for this one");
            return history;
        }
        
    }
}
