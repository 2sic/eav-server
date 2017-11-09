﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc.Tests;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Testing.Performance.json
{
    public class GenerateJsonForApps: Efc11TestBase
    {
        public int TestAppId = 261;
        public void GenerateJsonForApp261()
        {
            GenerateJsonForAllEntitiesOfApp(TestAppId);
        }


        public void LoadFromSql()
        {
            EntitiesAsJson = Db.ToSicEavEntities
                .Where(e => e.AttributeSet.AppId == TestAppId && e.Json != null)
                .Select(e => e.Json)
                .ToList();
        }



        public void LoadApp()
        {
            Package = Loader.AppPackage(TestAppId, parentLog: Log);   
        }

        internal AppDataPackage Package;
        internal List<string> EntitiesAsJson;

        internal JsonSerializer GetSerializer() => new JsonSerializer(Package, Log);

        public int SerializeAll(int repeat)
        {
            var count = 0;
            var ser = GetSerializer();
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
            var package = Loader.AppPackage(appid);
            var ser = new JsonSerializer(package, Log);
            var upd = package.List.ToDictionary(e => e.EntityId, e => ser.Serialize(e));

            var dbEnts = Db.ToSicEavEntities.Where(e => e.AttributeSet.AppId == appid).ToList();
            foreach (var dbEnt in dbEnts)
            {
                if (!upd.ContainsKey(dbEnt.EntityId)) continue;
                dbEnt.Json = upd[dbEnt.EntityId];
            }

            Db.UpdateRange(dbEnts);
            Db.SaveChanges("testingOnly", true);
        }


    }
}
