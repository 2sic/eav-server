using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc.Tests;
using AppState = ToSic.Eav.Apps.AppState;

//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Testing.Performance.json
{
    public class GenerateJsonForApps: Efc11TestBase
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
            EntitiesAsJson = Db.ToSicEavEntities
                .Where(e => e.AttributeSet.AppId == TestAppId && e.Json != null)
                .Select(e => e.Json)
                .ToList();
        }



        public void LoadApp()
        {
            Package = Loader.AppState(TestAppId, false);   
        }

        internal AppState Package;
        internal List<string> EntitiesAsJson;

        public int SerializeAll(int repeat)
        {
            var count = 0;
            var ser = Build<JsonSerializer>().Init(Package, Log);
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
            var package = Loader.AppState(appid, false);
            var ser = Build<JsonSerializer>().Init(package, Log);
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
