using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.Repository.Efc.Tests.Mocks;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class VersioningTests: TestBaseDiEavFullAndDb
    {
        #region Test Data
        public class VersioningTestSpecs: IAppIdentity
        {
            public int ZoneId => 4;
            public int AppId => 3012;
        }

        public VersioningTestSpecs Specs = new VersioningTestSpecs();

        private int TestItemWithCa20Changes = 20853;
        private int TestItemWithCa20ChangesCount = 22;
        private int ItemToRestoreToV2 = 20856;

        public IImportExportEnvironment Environment ;

        #endregion

        public VersioningTests()
        {
            Environment = Build<ImportExportEnvironmentMock>();
        }

        // todo: move tests to tests of ToSic.Eav.Apps
        [TestMethod]
        public void DevPc2dmRestoreV2()
        {
            var id = ItemToRestoreToV2;
            var version = 2;
            var appManager = Build<AppManager>().Init(Specs, Log);
            var dc = Build<DbDataController>().Init(Specs.ZoneId, Specs.AppId, Log);
            var all = appManager.Entities.VersionHistory(id);  dc.Versioning.GetHistoryList(id, false);
            var vId = all.First(x => x.VersionNumber == version).ChangeSetId;

            appManager.Entities.VersionRestore(id, vId);
        }

        [TestMethod]
        public void GetHistoryTests()
        {
            GetHistoryTest(TestItemWithCa20Changes, TestItemWithCa20ChangesCount);
        }

        [TestMethod]
        public void GetVersion6OfItemWith20()
        {
            var id = TestItemWithCa20Changes;
            var version = 6;
            var _dbData = Build<DbDataController>().Init(Specs.ZoneId, null, Log);
            var all = _dbData.Versioning.GetHistoryList(id, false);
            var vId = all.First(x => x.VersionNumber == version).ChangeSetId;
            var vItem = _dbData.Versioning.GetItem(id, vId);
            Console.Write(vItem.Json);
        }


        

        private List<ItemHistory> GetHistoryTest(int entityId, int expectedCount)
        {
            var _dbData = Build<DbDataController>().Init(Specs.ZoneId, null, Log);
            var history = _dbData.Versioning.GetHistoryList(entityId, true);
            Assert.AreEqual(expectedCount, history.Count, $"should have {expectedCount} items in history for this one");
            return history;
        }
        
    }
}
