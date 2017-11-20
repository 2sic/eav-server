using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.DataSources.Tests.RelationshipFilterTests
{
    [TestClass]
    public partial class Tst_RelationshipFilterDS: RelationshipTestBase
    {

        // todo: necessary tests
        // title compare on string
        // title compare on int
        // title compare on bool?
        // title compare on relationship-title
        // non-title compare on string
        // non-title compare on int
        // parent filtering on child
        // child filtering on parent
        // having-children
        // having-exactly x-children
        // ...all compare modes: contains, first, none,...
        

        [TestMethod]
        public void DS_RelFil_ConstructionWorks()
        {
            var relFilt = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);
            Trace.Write(Log.Dump());
            Assert.IsNotNull(relFilt, "relFilt != null");
        }

        [TestMethod]
        public void DS_RelFil_NoConfigEmpty()
        {
            var relFilt = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);

            var result = relFilt.List.ToList();

            Trace.Write(Log.Dump());

            Assert.IsTrue(result.Count == 0, "result.Count == 0");
        }
        [TestMethod]
        public void DS_RelFil_NoConfigFallback()
        {
            var relFilt = BuildRelationshipFilter(TestConfig.Zone, TestConfig.AppForRelationshipTests, Company);
            relFilt.Attach(Constants.FallbackStreamName, relFilt.In[Constants.DefaultStreamName]);

            var result = relFilt.List.ToList();

            Trace.Write(Log.Dump());
            Assert.IsTrue(result.Count > 0, "count should be more than 0, as it should use fallback");
        }

        [TestMethod]
        public void DS_RelFil_Companies_Having_Category_Title() 
            => new RelationshipTest("basic-cat-having-title", Company, CompCat, CatWeb).Run(true);

        [TestMethod]
        public void DS_RelFil_Companies_Having_Category_Active() 
            => new RelationshipTest("basic-cat-having-title", Company, CompCat, "true", relAttribute: "Active").Run(true);


        [Ignore]
        [TestMethod]
        public void DS_RelFil_Categories_OfCompanies() 
            => new RelationshipTest("basic-cat-having-title", Category, CompCat).Run(true);
    }
}
