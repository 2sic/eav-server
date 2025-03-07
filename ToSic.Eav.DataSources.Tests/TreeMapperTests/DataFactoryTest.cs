using ToSic.Eav.Data.Build;


namespace ToSic.Eav.DataSourceTests.TreeMapperTests;

[TestClass]
public class DataFactoryTest: TestBaseEavDataSource
{
    [TestMethod]
    public void ChildrenRelationships()
    {
        var builder = GetService<IDataFactory>().New();

        var parentRaw = new RawItemWithOneParentAndManyChildren(1, Guid.Empty, 0, [101, 102]);

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
        var getTitle = parent.Entity.GetTac("Title");
        IsNotNull(getTitle);
        AreEqual(getTitle, parentRaw.Title);

        var childrenProperty = parent.Entity.GetTac(childrenField);
        IsNotNull(childrenProperty);
        var childrenList = childrenProperty as IEnumerable<IEntity>;
        IsNotNull(childrenList);
        AreEqual(2, childrenList.Count());
        AreEqual(101, childrenList.First().EntityId);
        AreEqual(102, childrenList.Skip(1).First().EntityId);
    }
}