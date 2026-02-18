using static ToSic.Eav.DataSource.Linking.DsLinkingTestData;

namespace ToSic.Eav.DataSource.Linking;

public class DsLinkingWithMore
{

    [Fact]
    public void WithMore1()
    {
        var original = GetWithMockDataSource();
        var withMore = original.WithMoreTac([GetWithMockDataSource("More1")]);
        NotEqual(original, withMore);
        Single(withMore.MoreTac());
    }

    [Fact]
    public void WithMore2()
    {
        var original = GetWithMockDataSource();
        var withMore = original.WithMoreTac([
            GetWithMockDataSource("More1"),
            GetWithMockDataSource("More2")
        ]);
        NotEqual(original, withMore);
        Equal(2, withMore.MoreTac().Count());
    }

    [Fact]
    public void WithMore2Steps()
    {
        var original = GetWithMockDataSource();
        var withMore1 = original
            .WithMoreTac([GetWithMockDataSource("More1")]);
        var withMore2 = withMore1
            .WithMoreTac([GetWithMockDataSource("More2")]);

        NotEqual(original, withMore2);
        Equal(2, withMore2.MoreTac().Count());
    }

    private static (IDataSourceLink original, IDataSourceLink withMore2, IDataSourceLink withMoreStacked) GetWithMoreHierarchical()
    {
        var original = GetWithMockDataSource();
        var withMore2 = original
            .WithMoreTac([GetWithMockDataSource("More1"), GetWithMockDataSource("More2")]);
        var withMoreStacked = original
            .WithMoreTac([withMore2]);
        return (original, withMore2, withMoreStacked);
    }

    [Fact]
    public void WithMoreHierarchical()
    {
        var (original, withMore1, withMoreStacked) = GetWithMoreHierarchical();

        NotEqual(original, withMoreStacked);
        NotEqual(withMore1, withMoreStacked);

        Single(withMoreStacked.MoreTac());
        var first = withMoreStacked.MoreTac().First();
        Equal(2, first.MoreTac().Count());
    }

    [Fact]
    public void FlattenBasic()
    {
        var original = GetWithMockDataSource();
        var flattened = original.FlattenTac();
        Single(flattened);
    }

    [Fact]
    public void FlattenHierarchical()
    {
        var (_, withMore2, withMoreStacked) = GetWithMoreHierarchical();

        var flattenedWithMore2 = withMore2.FlattenTac();
        Equal(3, flattenedWithMore2.Count());

        var flattenedStack = withMoreStacked.FlattenTac();

        Equal(4, flattenedStack.Count());
    }
}
