using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.EntityFilters
{
    [TestClass]
    public class EntityTypeFilter_Test
    {
        [TestMethod]
        public void EntityTypeFilter_FindAllIfAllApply()
        {
            var vf = CreateEntityTypeFilterForTesting(1000);
            vf.TypeName = "Person";
            Assert.AreEqual(1000, vf.Immutable.Count(), "Should find all");
        }

        [TestMethod]
        public void EntityTypeFilter_FindNoneIfNoneApply()
        {
            var vf = CreateEntityTypeFilterForTesting(1000);
            vf.TypeName = "Category";
            Assert.AreEqual(0, vf.Immutable.Count(), "Should find all");
        }




        public static EntityTypeFilter CreateEntityTypeFilterForTesting(int testItemsInRootSource)
        {
            var ds = DataTableTst.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = Factory.Resolve<DataSourceFactory>().GetDataSource<EntityTypeFilter>(ds);
            return filtered;
        }
    }
}
