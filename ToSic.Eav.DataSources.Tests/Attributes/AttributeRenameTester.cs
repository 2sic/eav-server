using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    internal class AttributeRenameTester: EavTestBase
    {
        public readonly AttributeRename Original;
        public readonly AttributeRename Changed;
        public readonly IEntity CItem;
        public readonly List<IEntity> CList;
        public readonly IEntity OItem;

        public AttributeRenameTester(string map, bool preserve = true)
        {
            Original = CreateRenamer(10);
            Changed = CreateRenamer(10);
            if (map != null)
                Changed.AttributeMap = map;
            Changed.KeepOtherAttributes = preserve;

            CList = Changed.ListForTests().ToList();
            CItem = CList.First();
            OItem = Original.ListForTests().First();
        }

        internal void AssertValues(string fieldOriginal, string fieldNew = null)
        {
            var original = OItem;
            var modified = CItem;
            Assert.AreNotEqual(original, modified, "This test should never receive the same items!");
            fieldNew = fieldNew ?? fieldOriginal;
            Assert.AreEqual(
                original.Value<string>(fieldOriginal),
                modified.Value<string>(fieldNew), $"Renamed values on field '{fieldOriginal}' should match '{fieldNew}'");

        }

        public static AttributeRename CreateRenamer(int testItemsInRootSource)
        {
            var ds = DataTablePerson.Generate(testItemsInRootSource, 1001);
            var filtered = Resolve<DataSourceFactory>().GetDataSource<AttributeRename>(new AppIdentity(1, 1), ds);
            return filtered;
        }
    }
}
