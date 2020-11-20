using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.RelationshipFilterTests
{
    public class RelationshipTestBase: EavTestBase
    {
        #region Const Values for testing

        protected const string Company = "Company";
        protected const string Category = "Category";
        protected const string Country = "Country";
        protected const string Person = "Person";

        protected const string CompCat = "Categories";
        protected const string CatWeb = "Web";
        protected const string CatGreen = "Green";
        protected const string CompInexistingProp = "InexistingProperty";


        protected const string DefSeparator = ",";
        protected const string AltSeparator = "|";

        #endregion


        protected ILog Log { get; } = new Log("Tst.DSRelF");




        /// <summary>
        /// Build a relationship filter and configure it using the API
        /// </summary>
        /// <returns></returns>
        protected RelationshipFilter FilterWithApi(string type, 
            string relationship = null, 
            string filter = null, 
            string relAttrib = null,
            string compareMode = null,
            string separator = null,
        string direction = null)
        {
            var relApi = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, type);

            if (relationship != null) relApi.Relationship = relationship;
            if (filter != null) relApi.Filter = filter;
            if (relAttrib != null) relApi.CompareAttribute = relAttrib;
            if (compareMode != null) relApi.CompareMode = compareMode;
            if (separator != null) relApi.Separator = separator;
            if (direction != null) relApi.ChildOrParent = direction;

            return relApi;
        }

        /// <summary>
        /// Build a filter using a configuration/settings object to provide configuration
        /// </summary>
        /// <returns></returns>
        protected RelationshipFilter FilterWithConfig(string type, 
            string relationship = null, 
            string filter = null,
            string relAttrib = null,
            string compareMode = null,
            string separator = null,
            string direction = null)
        {
            var settings = new Dictionary<string, object> { { "Title", "..." } };

            void MaybeAddValue(RelationshipFilter.Settings key, string value) { if (value != null) settings.Add(key.ToString(), value); }
            MaybeAddValue(RelationshipFilter.Settings.Relationship, relationship);
            MaybeAddValue(RelationshipFilter.Settings.Filter, filter);
            MaybeAddValue(RelationshipFilter.Settings.AttributeOnRelationship, relAttrib);
            MaybeAddValue(RelationshipFilter.Settings.Comparison, compareMode);
            MaybeAddValue(RelationshipFilter.Settings.Separator, separator);
            MaybeAddValue(RelationshipFilter.Settings.Direction, direction);

            var config = BuildConfigurationProvider("Settings", settings);
            return BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company, config);
        }




        protected RelationshipFilter BuildRelationshipFilter(int zone, int app, string primaryType, ILookUpEngine config = null)
        {
            var baseDs = Resolve<DataSourceFactory>().GetPublishing(new AppIdentity(zone, app), configProvider: config);
            var appDs = Resolve<DataSourceFactory>().GetDataSource<App>(baseDs);

            // micro tests to ensure we have the right app etc.
            Assert.IsTrue(appDs.List.Count() > 20, "appDs.List.Count() > 20");

            var item731 = appDs.List.FindRepoId(731);
            Assert.IsNotNull(item731, "expecting item 731");
            var title = item731.GetBestTitle();
            Assert.AreEqual(title, "2sic", "item 731 should have title '2sic'");

            Assert.IsTrue(appDs.Out.ContainsKey(primaryType), $"app should contain stream of {primaryType}");

            var stream = appDs[primaryType];

            Assert.IsTrue(stream.Immutable.Count > 0, "stream.List.Count() > 0");

            var relFilt = Resolve<DataSourceFactory>().GetDataSource<RelationshipFilter>(new AppIdentity(0, 0), null, 
                appDs.Configuration.LookUps/*, parentLog: Log*/);
            relFilt.Attach(Constants.DefaultStreamName, stream);
            return relFilt;
        }


        protected static LookUpEngine BuildConfigurationProvider(string name, Dictionary<string, object> vals)
        {
            var vc = BuildConfigurationProvider();
            vc.Add(LookUpTestData.BuildLookUpEntity("Settings", vals));
            return vc;
        }

        protected static LookUpEngine BuildConfigurationProvider()
        {
            var vc = LookUpTestData.AppSetAndRes();
            return vc;
        }

    }
}
