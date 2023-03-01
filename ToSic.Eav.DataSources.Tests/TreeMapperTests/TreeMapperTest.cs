﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.New;
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

            var parentsRaw = new NewItemWithOneParentAndManyChildren(1, Guid.Empty, 0, new List<int> { 101, 102 });
            var parents = builder.Prepare(parentsRaw);

            var childrenRaw = new List<NewItemWithOneParentAndManyChildren>
            {
                new NewItemWithOneParentAndManyChildren(101, Guid.Empty, 0, null),
                new NewItemWithOneParentAndManyChildren(102, Guid.Empty, 0, null),
            };
            var children = builder.Prepare(childrenRaw);

            var mapper = GetService<TreeMapper>();

            const string childrenField = "Children";
            var result = mapper.AddOneRelationship(childrenField,
                new List<(NewEntitySet<string>, List<int>)> { (new NewEntitySet<string>("dummy", parents.Entity), parentsRaw.ChildrenIds) },
                children.Select(c => (c.Entity, c.Entity.EntityId)).ToList()
            );
            var updatedParents = result.First();

            // Control - to be sure the test can make sense
            var getTitle = updatedParents.Entity.GetBestValue("Title", Array.Empty<string>());
            Assert.IsNotNull(getTitle);

            var childrenProperty = updatedParents.Entity.GetBestValue(childrenField, Array.Empty<string>());
            Assert.IsNotNull(childrenProperty);


        }
    }
}
