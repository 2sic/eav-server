using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.RelationshipTests;
using ToSic.Lib.Logging;

using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.RelationshipFilterTests
{
    public class RelationshipTestBase: TestBaseDiEavFullAndDb
    {
        #region Const Values for testing

        public const string CompCat = "Categories";
        public const string CatWeb = "Web";
        public const string CatGreen = "Green";
        public const string CompInexistingProp = "InexistingProperty";


        protected const string DefSeparator = ",";
        protected const string AltSeparator = "|";

        #endregion


        protected new ILog Log { get; } = new Log("Tst.DSRelF");




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
            var relApi = BuildRelationshipFilter(type);

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
            var settings = new Dictionary<string, object> { { Attributes.TitleNiceName, "..." } };

            void MaybeAddValueStr(string key, string value) { if (value != null) settings.Add(key, value); }
            MaybeAddValueStr(nameof(RelationshipFilter.Relationship), relationship);
            MaybeAddValueStr(nameof(RelationshipFilter.Filter), filter);
            MaybeAddValueStr(RelationshipFilter.FieldAttributeOnRelationship, relAttrib);
            MaybeAddValueStr(RelationshipFilter.FieldComparison, compareMode);
            MaybeAddValueStr(nameof(RelationshipFilter.Separator), separator);
            MaybeAddValueStr(RelationshipFilter.FieldDirection, direction);

            var config = BuildConfigurationProvider(settings);
            return BuildRelationshipFilter(RelationshipTestSpecs.Company, config);
        }




        protected RelationshipFilter BuildRelationshipFilter(string primaryType, ILookUpEngine config = null)
        {
            var baseDs = DataSourceFactory.GetPublishing(RelationshipTestSpecs.AppIdentity, configProvider: config);
            var appDs = CreateDataSource<App>(baseDs);

            // micro tests to ensure we have the right app etc.
            Assert.IsTrue(appDs.ListForTests().Count() > 20, "appDs.List.Count() > 20");

            var item731 = appDs.ListForTests().FindRepoId(731);
            Assert.IsNotNull(item731, "expecting item 731");
            var title = item731.GetBestTitle();
            Assert.AreEqual(title, "2sic", "item 731 should have title '2sic'");

            Assert.IsTrue(appDs.Out.ContainsKey(primaryType), $"app should contain stream of {primaryType}");

            var stream = appDs[primaryType];

            Assert.IsTrue(stream.ListForTests().Any(), "stream.List.Count() > 0");

            var relFilt = CreateDataSource<RelationshipFilter>(appDs.Configuration.LookUpEngine);
            relFilt.AttachForTests(DataSourceConstants.DefaultStreamName, stream);
            return relFilt;
        }


        protected LookUpEngine BuildConfigurationProvider(Dictionary<string, object> vals)
        {
            var testData = new LookUpTestData(GetService<DataBuilder>());
            var vc = testData.AppSetAndRes();
            vc.Add(testData.BuildLookUpEntity(DataSource.MyConfiguration, vals));
            return vc;
        }

    }
}
