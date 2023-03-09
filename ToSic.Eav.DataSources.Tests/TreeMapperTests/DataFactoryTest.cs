using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Process;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.TreeMapperTests
{
    [TestClass]
    public class DataFactoryTest: TestBaseEavDataSource
    {
        [TestMethod]
        public void ChildrenRelationships()
        {
            var builder = GetService<IDataFactory>();

            var parentRaw = new RawItemWithOneParentAndManyChildren(1, Guid.Empty, 0, new List<int> { 101, 102 });

            var allRaw = new List<RawItemWithOneParentAndManyChildren>
            {
                // the parent
                parentRaw,
                // the children
                new RawItemWithOneParentAndManyChildren(101, Guid.Empty, 0, null),
                new RawItemWithOneParentAndManyChildren(102, Guid.Empty, 0, null),
            };
            var all = builder.Create(allRaw);

            const string childrenField = "Children";
            var parent = all.First();

            // Control - to be sure the test can make sense
            var getTitle = parent.Entity.GetBestValue("Title", Array.Empty<string>());
            Assert.IsNotNull(getTitle);
            Assert.AreEqual(getTitle, parentRaw.Title);

            var childrenProperty = parent.Entity.GetBestValue(childrenField, Array.Empty<string>());
            Assert.IsNotNull(childrenProperty);
            var childrenList = childrenProperty as IEnumerable<IEntity>;
            Assert.IsNotNull(childrenList);
            Assert.AreEqual(2, childrenList.Count());
            Assert.AreEqual(101, childrenList.First().EntityId);
            Assert.AreEqual(102, childrenList.Skip(1).First().EntityId);
        }
    }
}
