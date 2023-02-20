using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.TreeMapperTests
{
    [TestClass]
    public class TreeMapperTest: TestBaseEavDataSource
    {
        [TestMethod]
        public void JustAttachChildrenMappedFromParent()
        {
            var builder = GetService<IDataBuilder>();

            var parentsRaw = new RawItemWithOneParentAndManyChildren(1, Guid.Empty, 0, new List<int> { 101, 102 });
            var parents = builder.Create(parentsRaw);

            var childrenRaw = new List<RawItemWithOneParentAndManyChildren>
            {
                new RawItemWithOneParentAndManyChildren(101, Guid.Empty, 0, null),
                new RawItemWithOneParentAndManyChildren(102, Guid.Empty, 0, null),
            };
            var children = builder.CreateMany(childrenRaw);

            var mapper = GetService<TreeMapper>();

            const string childrenField = "Children";
            var result = mapper.AddSomeRelationshipsWIP(childrenField,
                new List<(IEntity, List<int>)> { (parents, parentsRaw.ChildrenIds) },
                children.Select(c => (c, c.EntityId)).ToList()
            );
            parents = result.First();

            // Control - to be sure the test can make sense
            var getTitle = parents.GetBestValue("Title", Array.Empty<string>());
            Assert.IsNotNull(getTitle);

            var childrenProperty = parents.GetBestValue(childrenField, Array.Empty<string>());
            Assert.IsNotNull(childrenProperty);


        }
    }
}
