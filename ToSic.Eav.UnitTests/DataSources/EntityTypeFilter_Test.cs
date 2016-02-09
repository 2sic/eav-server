using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class EntityTypeFilter_Test
    {
        [TestMethod]
        public void EntityTypeFilter_FindAllIfAllApply()
        {
            var vf = CreateEntityTypeFilterForTesting(1000);
            vf.TypeName = "Person";
            Assert.AreEqual(1000, vf.List.Count, "Should find all");
        }

        [TestMethod]
        public void EntityTypeFilter_FindNoneIfNoneApply()
        {
            var vf = CreateEntityTypeFilterForTesting(1000);
            vf.TypeName = "Category";
            Assert.AreEqual(0, vf.List.Count, "Should find all");
        }




        public static EntityTypeFilter CreateEntityTypeFilterForTesting(int testItemsInRootSource)
        {
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = DataSource.GetDataSource<EntityTypeFilter>(0, 0, ds);
            return filtered;
        }
    }
}
