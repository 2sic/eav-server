﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.RelationshipTests;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
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
            var settings = new Dictionary<string, object> { { "Title", "..." } };

            void MaybeAddValue(RelationshipFilter.Settings key, string value) { if (value != null) settings.Add(key.ToString(), value); }
            MaybeAddValue(RelationshipFilter.Settings.Relationship, relationship);
            MaybeAddValue(RelationshipFilter.Settings.Filter, filter);
            MaybeAddValue(RelationshipFilter.Settings.AttributeOnRelationship, relAttrib);
            MaybeAddValue(RelationshipFilter.Settings.Comparison, compareMode);
            MaybeAddValue(RelationshipFilter.Settings.Separator, separator);
            MaybeAddValue(RelationshipFilter.Settings.Direction, direction);

            var config = BuildConfigurationProvider(settings);
            return BuildRelationshipFilter(RelationshipTestSpecs.Company, config);
        }




        protected RelationshipFilter BuildRelationshipFilter(string primaryType, ILookUpEngine config = null)
        {
            var baseDs = DataSourceFactory.GetPublishing(RelationshipTestSpecs.AppIdentity, configProvider: config);
            var appDs = DataSourceFactory.GetDataSource<App>(baseDs);

            // micro tests to ensure we have the right app etc.
            Assert.IsTrue(appDs.ListForTests().Count() > 20, "appDs.List.Count() > 20");

            var item731 = appDs.ListForTests().FindRepoId(731);
            Assert.IsNotNull(item731, "expecting item 731");
            var title = item731.GetBestTitle();
            Assert.AreEqual(title, "2sic", "item 731 should have title '2sic'");

            Assert.IsTrue(appDs.Out.ContainsKey(primaryType), $"app should contain stream of {primaryType}");

            var stream = appDs[primaryType];

            Assert.IsTrue(stream.ListForTests().Any(), "stream.List.Count() > 0");

            var relFilt = DataSourceFactory.GetDataSource<RelationshipFilter>(new AppIdentity(0, 0), null, 
                appDs.Configuration.LookUpEngine);
            relFilt.AttachForTests(Constants.DefaultStreamName, stream);
            return relFilt;
        }


        protected static LookUpEngine BuildConfigurationProvider(Dictionary<string, object> vals)
        {
            var vc = LookUpTestData.AppSetAndRes();
            vc.Add(LookUpTestData.BuildLookUpEntity("Settings", vals));
            return vc;
        }

    }
}