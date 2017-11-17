using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data.Query;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;
using ToSic.Eav.TokenEngine.Tests.TestData;

namespace ToSic.Eav.DataSources.Tests.RelationshipFilterTests
{
    [TestClass]
    public class Tst_RelationshipFilterDS
    {
        #region Const Values for testing

        private const string Company = "Company";
        private const string Category = "Category";
        private const string Country = "Country";
        private const string Person = "Person";

        private const string CompCat = "Categories";
        private const string CatWeb = "Web";


        private Log Log { get; } = new Log("Tst.DSRelF");

        #endregion

        [TestMethod]
        public void DS_RelFil_Basic()
        {
            var relFilt = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);

            Trace.Write(Log.Dump());

            Assert.IsNotNull(relFilt, "relFilt != null");
        }

        [TestMethod]
        public void DS_RelFil_NoConfigShouldBeEmpty()
        {
            var relFilt = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);

            var result = relFilt.List.ToList();

            Trace.Write(Log.Dump());

            Assert.IsTrue(result.Count == 0, "result.Count == 0");
        }
        [TestMethod]
        public void DS_RelFil_NoConfigShouldBeFallback()
        {
            var relFilt = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);
            relFilt.Attach(Constants.FallbackStreamName, relFilt.In[Constants.DefaultStreamName]);

            var result = relFilt.List.ToList();

            Trace.Write(Log.Dump());
            Assert.IsTrue(result.Count > 0, "count should be more than 0, as it should use fallback");
        }

        [TestMethod]
        public void DS_RelFil_Companies_Having_Category()
        {
            var relApi = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);
            var allComps = relApi.In[Constants.DefaultStreamName].List.Count();

            relApi.Relationship = CompCat;
            relApi.Filter = CatWeb;
            var foundCount = relApi.List.Count();

            // Identical test with configuration providing
            var config = BuildConfigurationProvider("Settings", new Dictionary<string, object>
            {
                {"Title", "..."},
                {RelationshipFilter.SettingsRelationship, CompCat},
                {RelationshipFilter.SettingsFilter, CatWeb}
            });

            var relConfig = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company, config);
            var foundWithConfig = relConfig.List.Count();

            Trace.Write(Log.Dump());

            Assert.IsTrue(foundCount > 0, "foundCount > 0");
            Assert.IsTrue(foundCount < allComps, "foundCount < allComps");
            Assert.AreEqual(foundCount, foundWithConfig, "api and config should be the same");
        }



        [TestMethod]
        public void DS_RelFil_Companies_Having_Category_WithTitleAttrib_ObjApi()
        {
            var relFilt = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);

            var allComps = relFilt.In[Constants.DefaultStreamName].List.Count();

            relFilt.Relationship = "Categories";
            relFilt.Filter = "Web";

            var result = relFilt.List.ToList();
            var foundCount = result.Count();

            Trace.Write(Log.Dump());

            Assert.IsTrue(foundCount > 0, "foundCount > 0");
            Assert.IsTrue(foundCount < allComps, "foundCount < allComps");
        }




        private RelationshipFilter BuildRelationshipFilter(int zone, int app, string primaryType, IValueCollectionProvider config = null)
        {
            var baseDs = DataSource.GetInitialDataSource(zone, app, configProvider: config, parentLog: Log);
            var appDs = DataSource.GetDataSource<App>(zone, app, baseDs, parentLog: Log);

            // micro tests to ensure we have the right app etc.
            Assert.IsTrue(appDs.List.Count() > 20, "appDs.List.Count() > 20");

            Assert.IsTrue(appDs.List.One(731)?.GetBestTitle() == "2sic", "expecting item 731");

            Assert.IsTrue(appDs.Out.ContainsKey(primaryType), $"app should contain stream of {primaryType}");

            var stream = appDs[primaryType];

            Assert.IsTrue(stream.List.Count() > 0, "stream.List.Count() > 0");

            var relFilt = DataSource.GetDataSource<RelationshipFilter>(zone, app, null, appDs.ConfigurationProvider, parentLog: Log);
            relFilt.Attach(Constants.DefaultStreamName, stream);
            return relFilt;
        }


        private static ValueCollectionProvider BuildConfigurationProvider(string name, Dictionary<string, object> vals)
        {
            var vc = BuildConfigurationProvider();
            vc.Add(DemoConfigs.BuildValueProvider("Settings",vals));
            return vc;
        }

        private static ValueCollectionProvider BuildConfigurationProvider()
        {
            var vc = DemoConfigs.AppSetAndRes();
            return vc;
        }

    }
}
