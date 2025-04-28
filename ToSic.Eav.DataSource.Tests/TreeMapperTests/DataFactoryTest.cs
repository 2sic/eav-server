using ToSic.Eav.Data.Build;

namespace ToSic.Eav.TreeMapperTests;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class DataFactoryTest(IDataFactory dataFactory)
{
    [Fact]
    public void ChildrenRelationships()
    {
        var builder = dataFactory.New();

        var parentRaw = new RawItemWithOneParentAndManyChildren(1, Guid.Empty, 0, [101, 102]);

        var allRaw = new List<RawItemWithOneParentAndManyChildren>
        {
            // the parent
            parentRaw,
            // the children
            new(101, Guid.Empty, 0, null),
            new(102, Guid.Empty, 0, null),
        };
        var all = builder.Create(allRaw);

        const string childrenField = "Children";
        var parent = all.First();

        // Control - to be sure the test can make sense
        var getTitle = parent.Entity.GetTac("Title");
        NotNull(getTitle);
        Equal(getTitle, parentRaw.Title);

        var childrenProperty = parent.Entity.GetTac(childrenField);
        NotNull(childrenProperty);
        var childrenList = childrenProperty as IEnumerable<IEntity>;
        NotNull(childrenList);
        Equal(2, childrenList.Count());
        Equal(101, childrenList.First().EntityId);
        Equal(102, childrenList.Skip(1).First().EntityId);
    }
}