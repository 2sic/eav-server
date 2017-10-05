using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Persistence.Efc.Tests.json
{
    [TestClass]
    public class GenerateJsonForApps:Efc11TestBase
    {
        [TestMethod]
        [Ignore]
        public void GenerateJsonForApp261()
        {
            GenerateJsonForAllEntitiesOfApp(261);
        }

        private void GenerateJsonForAllEntitiesOfApp(int appid)
        {
            var package = Loader.AppPackage(appid);
            var ser = SerializerOfApp(package);
            var upd = package.List.ToDictionary(e => e.EntityId, e => ser.Serialize(e));

            var dbEnts = Db.ToSicEavEntities.Where(e => e.AttributeSet.AppId == appid).ToList();
            foreach (var dbEnt in dbEnts)
            {
                if (!upd.ContainsKey(dbEnt.EntityId)) continue;
                dbEnt.Json = upd[dbEnt.EntityId];
            }

            Db.UpdateRange(dbEnts);
            Db.SaveChanges();
        }
    }
}
