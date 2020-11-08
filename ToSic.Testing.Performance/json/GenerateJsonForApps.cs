using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Core.Tests;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc.Tests;
using AppState = ToSic.Eav.Apps.AppState;

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
            Package = Loader.AppState(TestAppId, parentLog: Log);   
        }

        internal AppState Package;
        internal List<string> EntitiesAsJson;

        public int SerializeAll(int repeat)
        {
            var count = 0;
            var ser = EavTestBase.Resolve<JsonSerializer>().Init(Package, Log);
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
            var package = Loader.AppState(appid);
            var ser = EavTestBase.Resolve<JsonSerializer>().Init(package, Log);
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
