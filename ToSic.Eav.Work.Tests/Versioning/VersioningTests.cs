using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.DI;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Versioning;

[Startup(typeof(StartupTestWork))]
public class VersioningTests(Generator<DbStorage> dbDataGenerator, GenWorkDb<WorkEntityVersioning> workEntityVersioning, ITestOutputHelper output)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    #region Test Data
    public class VersioningTestSpecs: IAppIdentity
    {
        public int ZoneId => 4;
        public int AppId => 3012;
    }

    public VersioningTestSpecs Specs = new();

    private int TestItemWithCa20Changes = 20853;
    private int TestItemWithCa20ChangesCount = 22;
    private int ItemToRestoreToV2 = 20856;

    public IImportExportEnvironment Environment ;

    #endregion

    // TODO: move tests to tests of ToSic.Eav.Apps
    // ATM this test doesn't work
    [Fact] public void TestingDbRestoreV2CurrentlyNotReallyDoingWhatItShould()
    {
        var id = ItemToRestoreToV2;
        var version = 2;

        //var appManager = GetService<AppManager>().Init(Specs);
        var versionSvc = workEntityVersioning.New(appId: Specs.AppId);
        var dc = dbDataGenerator.New().Init(Specs.ZoneId, Specs.AppId);
        //var all = appManager.Entities.VersionHistory(id);
        dc.Versioning.GetHistoryList(id, false);
        var all = versionSvc.VersionHistory(id);
        var vId = all.First(x => x.VersionNumber == version).ChangeSetId;

        //appManager.Entities.VersionRestore(id, vId);
        //versionSvc.VersionRestore(id, vId);
    }

    [Fact] public void GetHistoryTests() =>
        GetHistoryTest(TestItemWithCa20Changes, TestItemWithCa20ChangesCount);

    [Fact] public void GetVersion6OfItemWith20()
    {
        var id = TestItemWithCa20Changes;
        var version = 6;
        var _dbData = dbDataGenerator.New().Init(Specs.ZoneId, null);
        var all = _dbData.Versioning.GetHistoryList(id, false);
        var vId = all.First(x => x.VersionNumber == version).ChangeSetId;
        var vItem = _dbData.Versioning.GetItem(id, vId);
        output.WriteLine(vItem.Json);
    }


        

    private List<ItemHistory> GetHistoryTest(int entityId, int expectedCount)
    {
        var dbData = dbDataGenerator.New().Init(Specs.ZoneId, null);
        var history = dbData.Versioning.GetHistoryList(entityId, true);
        Equal(expectedCount, history.Count);//, $"should have {expectedCount} items in history for this one");
        return history;
    }
        
}