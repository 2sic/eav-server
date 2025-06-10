﻿using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Serialization.Sys;

//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Testing.Performance.json;

public class TestGenerateJsonForApps(EavDbContext db, Generator<JsonSerializer> jsonSerializerGen, Generator<EfcAppLoaderService> loaderGenerator)
{
    public const int OldTestAppOn2dmDnn = 269; // app on 2dms DNN previously - the tractor app on http://2sexycontent.2dm.2sic/owner-filter/tractor-app;

    public List<string> LoadJsonsFromSql(int appId)
    {
        var entitiesAsJson = db.TsDynDataEntities
            .Where(e => e.AppId == appId && e.Json != null)
            .Select(e => e.Json)
            .ToList();
        return entitiesAsJson;
    }



    public void LoadApp(int appId)
    {
        AppReaderRaw = ((IAppsAndZonesLoaderWithRaw)loaderGenerator.New()).AppReaderRaw(appId, new());
    }

    internal IAppReader AppReaderRaw;

    public int SerializeAll(int repeat, List<string> entitiesAsJson)
    {
        var count = 0;
        var ser = jsonSerializerGen.New().SetApp(AppReaderRaw);
        for (var i = 0; i < repeat; i++)
        {
            var ents = ser.Deserialize(entitiesAsJson);
            count += ents.Count;
        }
        return count;
        //Trace.Write($"found {ents.Count} items");
        //Assert.IsTrue(ents.Count > 100, "should find a lot of items");
    }

    /// <summary>
    /// places json-data into all entities of an app, with the own data stored inside
    /// </summary>
    /// <param name="appId"></param>
    public void GenerateJsonForAllEntitiesOfApp(int appId)
    {
        var package = ((IAppsAndZonesLoaderWithRaw)loaderGenerator.New()).AppReaderRaw(appId, new());
        var ser = jsonSerializerGen.New().SetApp(package);
        var upd = package.List.ToDictionary(e => e.EntityId, e => ser.Serialize(e));

        var dbEnts = db.TsDynDataEntities
            .Where(e => e.AppId == appId)
            .ToList();
        foreach (var dbEnt in dbEnts)
        {
            if (!upd.TryGetValue(dbEnt.EntityId, out var value))
                continue;
            dbEnt.Json = value;
        }

        db.UpdateRange(dbEnts);
        db.SaveChanges("testingOnly", true);
    }


}