using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.EntityFilters
{
    [TestClass]
    public class EntityTypeFilter_Test: TestBaseDiEavFullAndDb
    {
        [TestMethod]
        public void EntityTypeFilter_FindAllIfAllApply()
        {
            var vf = CreateEntityTypeFilterForTesting(1000);
            vf.TypeName = "Person";
            Assert.AreEqual(1000, vf.ListForTests().Count(), "Should find all");
        }

        [TestMethod]
        public void EntityTypeFilter_FindNoneIfNoneApply()
        {
            var vf = CreateEntityTypeFilterForTesting(1000);
            vf.TypeName = "Category";
            Assert.AreEqual(0, vf.ListForTests().Count(), "Should find all");
        }




        public EntityTypeFilter CreateEntityTypeFilterForTesting(int testItemsInRootSource)
        {
            var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001);
            var filtered = Build<DataSourceFactory>().GetDataSource<EntityTypeFilter>(ds);
            return filtered;
        }
    }
}
