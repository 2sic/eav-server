using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;
using static ToSic.Eav.DataSourceTests.RelationshipTests.MetadataTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests
{
    [TestClass]
    public class MetadataTests: TestBaseDiEavFullAndDb
    {
        private void TestMetadata(int expected, DataSources.Metadata ds) => Assert.AreEqual(expected, ds.ListForTests().Count(), $"should have {expected} md items");


        [TestMethod]
        public void MainOtherTotalMetadata() => TestMetadata(1, PrepareDs(TargetTypeOther));

        [TestMethod]
        public void MainTargetsTotalMetadata() => TestMetadata(ItemsCount, PrepareDs(TargetTypeMain));

        [TestMethod]
        public void MainTargetsHelpOnly() => TestMetadata(MainHelp, PrepareDs(TargetTypeMain, typeName: HelpTypeName));

        [TestMethod]
        public void MainTargetsPriceOnly() => TestMetadata(PricesTotal, PrepareDs(TargetTypeMain, typeName: PriceTypeName));

        [TestMethod]
        public void MainSingleItem() => TestMetadata(Item2P1HTotal, PrepareDs(TargetTypeMain, ids: new[] { Item2P1H }));

        [TestMethod]
        public void MainSingleItemPriceOnly() => TestMetadata(PricesTotal, PrepareDs(TargetTypeMain, typeName: PriceTypeName, ids: new[] { Item2P1H }));



        protected DataSources.Metadata PrepareDs(string appType = null, IEnumerable<int> ids = null, string typeName = null, ILookUpEngine lookUpEngine = null)
        {
            if(lookUpEngine == null) lookUpEngine = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes();

            var baseDs = DataSourceFactory.GetPublishing(AppIdentity, configProvider: lookUpEngine);
            var appDs = CreateDataSource<App>(baseDs);

            var inStream = FilterStreamByIds(ids, appDs.GetStream(appType));

            var childDs = CreateDataSource<DataSources.Metadata>(inStream);
            if (typeName != null)
                childDs.ContentTypeName = typeName;

            return childDs;
        }

    }
}
