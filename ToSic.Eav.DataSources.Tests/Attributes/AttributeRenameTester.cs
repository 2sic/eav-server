using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.Attributes
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

            CList = Changed.Immutable.ToList();
            CItem = CList.First();
            OItem = Original.Immutable.First();
        }

        internal void AssertValues(string fieldOriginal, string fieldNew = null)
        {
            var original = OItem;
            var modified = CItem;
            Assert.AreNotEqual(original, modified, "This test should never receive the same items!");
            fieldNew = fieldNew ?? fieldOriginal;
            Assert.AreEqual(
                original.GetBestValue<string>(fieldOriginal),
                modified.GetBestValue<string>(fieldNew), $"Renamed values on field '{fieldOriginal}' should match '{fieldNew}'");

        }

        public static AttributeRename CreateRenamer(int testItemsInRootSource)
        {
            var ds = DataTableTst.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = Resolve<DataSourceFactory>().GetDataSource<AttributeRename>(new AppIdentity(1, 1), ds);
            return filtered;
        }
    }
}
