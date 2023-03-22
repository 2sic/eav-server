using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.RelationshipTests;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSourceTests.RelationshipFilterTests
{
    [TestClass]
    public partial class RelationshipFilterTest: RelationshipTestBase
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
            var relFilt = BuildRelationshipFilter(RelationshipTestSpecs.Company);
            Trace.Write(Log.Dump());
            Assert.IsNotNull(relFilt, "relFilt != null");
        }

        [TestMethod]
        public void DS_RelFil_NoConfigEmpty()
        {
            var relFilt = BuildRelationshipFilter(RelationshipTestSpecs.Company);

            var result = relFilt.ListForTests().ToList();

            Trace.Write(Log.Dump());

            Assert.IsTrue(result.Count == 0, "result.Count == 0");
        }
        [TestMethod]
        public void DS_RelFil_NoConfigFallback()
        {
            var relFilt = BuildRelationshipFilter(RelationshipTestSpecs.Company);
            relFilt.AttachForTests(DataSourceConstants.StreamFallbackName, relFilt.InForTests()[DataSourceConstants.StreamDefaultName]);

            var result = relFilt.ListForTests().ToList();

            Trace.Write(Log.Dump());
            Assert.IsTrue(result.Count > 0, "count should be more than 0, as it should use fallback");
        }

        [TestMethod]
        public void DS_RelFil_Companies_Having_Category_Title() 
            => new RelationshipTestCase("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, CatWeb).Run(true);

        [TestMethod]
        public void DS_RelFil_Companies_Having_Category_Active() 
            => new RelationshipTestCase("basic-cat-having-title", RelationshipTestSpecs.Company, CompCat, "true", relAttribute: "Active").Run(true);

        [TestMethod]
        public void DS_RelFil_Companies_Having_InexistingProperty_Title() 
            => new RelationshipTestCase("basic-cat-having-inexisting-property", RelationshipTestSpecs.Company, CompInexistingProp, CatWeb).Run(false);

    }
}
