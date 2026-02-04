namespace ToSic.Eav.Data.Build.TreeMapperTests;

[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryTest(IDataFactory dataFactoryGenerator)
{
    [Fact]
    public void ChildrenRelationships()
    {
        var builder = dataFactoryGenerator.SpawnNew(new());

        var parentRaw = new RawItemWithOneParentAndManyChildren(1, Guid.Empty, 0, [101, 102]);

        var allRaw = new List<RawItemWithOneParentAndManyChildren>
        {
            // the parent
            parentRaw,
            // the children
            new(101, Guid.Empty, 0, null),
            new(102, Guid.Empty, 0, null),
        };
        var all = builder.Create(allRaw).ToList();

        const string childrenField = "Children";
        var parent = all.First();

        // Control - to be sure the test can make sense
        var getTitle = parent.Entity.GetTac("Title");
        NotNull(getTitle);
        Equal(getTitle, parentRaw.Title);

        var childrenProperty = parent.Entity.GetTac(childrenField);
        NotNull(childrenProperty);
        var childrenList = ((IEnumerable<IEntity>)childrenProperty).ToList();
        NotNull(childrenList);
        Equal(2, childrenList.Count());
        Equal(101, childrenList.First().EntityId);
        Equal(102, childrenList.Skip(1).First().EntityId);
    }
}