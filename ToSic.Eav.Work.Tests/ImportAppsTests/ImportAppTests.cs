using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.ImportExport.Internal.Zip;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.ImportAppsTests;

public class ImportAppTests(ZipImport zipImport, DbDataController dbData, ZoneManager zoneManager)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    #region Test Data

    internal class AppImportDef
    {
        public required string Name;
        public required string Guid;
        public required string Zip;
    }
    internal Dictionary<string, AppImportDef> Apps = new()
    {
        { "tile", new() { Name = "Tile", Guid = "efba5a03-2926-488e-a30d-0bf9b6541bbb", Zip = "2sxcApp_Tiles_01.02.00.zip"}},
        { "qr", new() { Name = "QR Code", Guid = "55e57a39-e506-416a-aed0-1c7459d31e86", Zip = "2sxcApp_QRCode_01.00.03.zip"}}

    };

    // Zone 5 is the /import-export on the eav-testing system
    public const int ZoneId = 5;

    #endregion
    [Fact]
    public void DeleteApp_Tile()
    {
        DeleteAnApp(Apps["tile"].Guid);
    }

    [Fact]
    public void ImportApp_Tile() => ImportAnApp("tile");

    [Fact]
    public void DeleteApp_Qr() => DeleteAnApp(Apps["qr"].Guid);

    [Fact]
    public void ImportApp_Qr() => ImportAnApp("qr");

    internal void ImportAnApp(string name)
    {
        // to be sure, clean up first
        DeleteAnApp(Apps[name].Guid);

        var baseTestPath = TestFiles.GetTestPath("");
        var testFileName = baseTestPath + "\\ImportAppsTests\\Import-Packages\\" + Apps[name].Zip;

        bool succeeded;

        using (var fsSource = new FileStream(testFileName, FileMode.Open, FileAccess.Read))
        {
            zipImport.Init(ZoneId, null, true);
            succeeded = zipImport.ImportZip(fsSource, baseTestPath + @"Temp\");
        }
        True(succeeded, "should succeed!");
    }


    private void DeleteAnApp(string appGuid)
    {
        var appList = dbData.Init(ZoneId, null).SqlDb.TsDynDataApps
            .Where(a => a.ZoneId == ZoneId)
            .ToList();
        var appId = appList.FirstOrDefault(a => a.Name == appGuid)?.AppId ?? 0;
        if (appId > 0)
            zoneManager.SetId(ZoneId).DeleteApp(appId, true);

    }
}