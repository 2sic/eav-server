using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Persistence.Efc.Tests.json
{
    [TestClass]
    public class GenerateJsonForApps:Efc11TestBase
    {
        [TestMethod]
        public void GenerateJsonForApp261()
        {
            GenerateJsonForAllEntitiesOfApp(261);
        }

        private void GenerateJsonForAllEntitiesOfApp(int appid)
        {
            var package = Loader.AppPackage(appid);

            var upd = package.List.Select(e => new {e.EntityId}).ToList();

        }
    }
}
