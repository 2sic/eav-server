using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    internal class AttributeRenameTester: TestServiceBase
    {
        public AttributeRename Original;
        public AttributeRename Changed;
        public IEntity CItem;
        public List<IEntity> CList;
        public IEntity OItem;

        public AttributeRenameTester(TestBaseEavDataSource parent): base(parent) {}

        public AttributeRenameTester Init(string map, bool preserve = true)
        {
            Original = CreateRenamer(10);
            Changed = CreateRenamer(10);
            if (map != null)
                Changed.AttributeMap = map;
            Changed.KeepOtherAttributes = preserve;

            CList = Changed.ListForTests().ToList();
            CItem = CList.First();
            OItem = Original.ListForTests().First();
            return this;
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

        public AttributeRename CreateRenamer(int testItemsInRootSource)
        {
            var ds = new DataTablePerson(Parent).Generate(testItemsInRootSource, 1001);
            var filtered = Parent.CreateDataSource<AttributeRename>(ds);
            return filtered;
        }
    }
}
