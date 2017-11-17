using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSources.Tests.RelationshipFilterTests
{
    [TestClass]
    public class RelationshipFilterDataSource
    {
        [TestMethod]
        public void DS_RelFil_Basic()
        {
            var zone = TestConfiguration.Zone;
            var app = TestConfiguration.AppForRelationshipTests;
            var primaryType = "Company";

            var relFilt = BuildRelationshipFilter(zone, app, primaryType);

            Assert.IsNotNull(relFilt, "relFilt != null");
        }

        private RelationshipFilter BuildRelationshipFilter(int zone, int app, string primaryType)
        {
            //LoadAppFromDb(zone, app);

            //var config = new UnitTests.ValueProvider.ValueCollectionProvider_Test().ValueCollection();

            //Assert.IsNotNull(config, "config != null");

            var baseDs =
                DataSource.GetInitialDataSource(zone, app);
            var appDs = DataSource.GetDataSource<App>(null, null, baseDs);

            Assert.IsTrue(appDs.List.Count() > 20, "appDs.List.Count() > 20");

            var relFilt = DataSource.GetDataSource<RelationshipFilter>(zone, app, null, baseDs.ConfigurationProvider);
            relFilt.Attach(Constants.DefaultStreamName, appDs[primaryType]);
            return relFilt;
        }


        //private void LoadAppFromDb(int zoneId, int appId)
        //{
        //    var ldr = Factory.Resolve<IRepositoryLoader>();
        //    var load = ldr.AppPackage(appId);
        //}
    }
}
