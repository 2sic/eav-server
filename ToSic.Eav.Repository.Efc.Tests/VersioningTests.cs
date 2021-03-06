﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.Repository.Efc.Tests.Mocks;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class VersioningTests: EavTestBase
    {

        public static ILog Log = new Log("TstVer");

        #region Test Data

        private int TestItemWithCa20Changes = 3893;
        private Guid ItemWithCa20Changes = new Guid("5BF76130-B386-4848-9F17-AAE567D50CF6");
        //private int TestItemWithCa100Changes = 4457;
        private Guid ItemWithCa100Changes = new Guid("23F24A0C-B4A2-43B4-ACBF-F5AC64549BF1");
        //private int TestItemWithCa9Changes = 7707;
        private Guid ItemWithCa9Changes = new Guid("3AB334D9-A99C-4C01-AAB7-C29E04274B5C");

        private TestValuesOnPc2Dm td = new TestValuesOnPc2Dm();
        private int DevPc2dmItemOnHome = 3269;

        // just a note: this is Portal 54 or something on Daniels test server, change it to your system if you want to run these tests!
        public const int ZoneId = 56;

        public IImportExportEnvironment Environment = new ImportExportEnvironmentMock();


        #endregion

        // todo: move tests to tests of ToSic.Eav.Apps
        [TestMethod]
        public void DevPc2dmRestoreV2()
        {
            var id = DevPc2dmItemOnHome;
            var version = 2;
            var _appManager = Resolve<AppManager>().Init(td.TestApp, Log);
            var dc = Resolve<DbDataController>().Init(td.ZoneId, td.AppId, Log);
            var all = _appManager.Entities.VersionHistory(id);  dc.Versioning.GetHistoryList(id, false);
            var vId = all.First(x => x.VersionNumber == version).ChangeSetId;

            _appManager.Entities.VersionRestore(DevPc2dmItemOnHome, vId);

        }

        [TestMethod]
        public void GetHistoryTests()
        {
            GetHistoryTest(TestItemWithCa20Changes, 31);
            // 2017-10-05 2dm disabled temporarily, as history was cleared
            //GetHistoryTest(TestItemWithCa100Changes, 102);
            //GetHistoryTest(TestItemWithCa9Changes, 9);
        }

        [TestMethod]
        public void GetVersion6OfItemWith20()
        {
            var id = TestItemWithCa20Changes;
            var version = 6;
            var _dbData = Resolve<DbDataController>().Init(ZoneId, null, Log);
            var all = _dbData.Versioning.GetHistoryList(id, false);
            var vId = all.First(x => x.VersionNumber == version).ChangeSetId;
            var vItem = _dbData.Versioning.GetItem(id, vId);
            Console.Write(vItem.Json);
        }


        

        private List<ItemHistory> GetHistoryTest(int entityId, int expectedCount)
        {
            var _dbData = Resolve<DbDataController>().Init(ZoneId, null, Log);
            var history = _dbData.Versioning.GetHistoryList(entityId, true);
            Assert.AreEqual(expectedCount, history.Count, $"should have {expectedCount} items in history for this one");
            return history;
        }
        
    }
}
