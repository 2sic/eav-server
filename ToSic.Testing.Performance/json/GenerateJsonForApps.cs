using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Sys;
using ToSic.Lib.DI;

//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Testing.Performance.json;

public class GenerateJsonForApps(EavDbContext Db, Generator<JsonSerializer> jsonSerializerGen, Generator<EfcAppLoaderService> loaderGenerator)
{
    public const int EmptyAppId = 3010;
    public const int HugeAppWith4000Items = 9;
    public const int OldTestAppOn2dmDnn = 269; // app on 2dms DNN previously - the tractor app on http://2sexycontent.2dm.2sic/owner-filter/tractor-app;
    public int TestAppId = EmptyAppId;
    public void GenerateJsonForApp261()
    {
        GenerateJsonForAllEntitiesOfApp(TestAppId);
    }


    public void LoadFromSql()
    {
        EntitiesAsJson = Db.TsDynDataEntities
            .Where(e => e.AppId == TestAppId && e.Json != null)
            .Select(e => e.Json)
            .ToList();
    }



    public void LoadApp()
    {
        AppReaderRaw = loaderGenerator.New().AppStateReaderRawTac(TestAppId);   
    }

    internal IAppReader AppReaderRaw;
    internal List<string> EntitiesAsJson;

    public int SerializeAll(int repeat)
    {
        var count = 0;
        var ser = jsonSerializerGen.New().SetApp(AppReaderRaw);
        for (var i = 0; i < repeat; i++)
        {
            var ents = ser.Deserialize(EntitiesAsJson);
            count += ents.Count;
        }
        return count;
        //Trace.Write($"found {ents.Count} items");
        //Assert.IsTrue(ents.Count > 100, "should find a lot of items");
    }

    /// <summary>
    /// places json-data into all entities of an app, with the own data stored inside
    /// </summary>
    /// <param name="appid"></param>
    private void GenerateJsonForAllEntitiesOfApp(int appid)
    {
        var package = loaderGenerator.New().AppStateReaderRawTac(appid);
        var ser = jsonSerializerGen.New().SetApp(package);
        var upd = package.List.ToDictionary(e => e.EntityId, e => ser.Serialize(e));

        var dbEnts = Db.TsDynDataEntities.Where(e => e.AppId == appid).ToList();
        foreach (var dbEnt in dbEnts)
        {
            if (!upd.ContainsKey(dbEnt.EntityId)) continue;
            dbEnt.Json = upd[dbEnt.EntityId];
        }

        Db.UpdateRange(dbEnts);
        Db.SaveChanges("testingOnly", true);
    }


}