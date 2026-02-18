using static ToSic.Eav.DataSource.DataSourceConstants;
using static ToSic.Eav.DataSource.Linking.DsLinkingTestData;

namespace ToSic.Eav.DataSource.Linking;

public class DsLinkingWithAnotherStream
{

    private static (IDataSourceLink original, IDataSourceLink withStream) GetWithAnotherStream(string name = Rename)
    {
        var original = GetWithMockDataSource();
        var withStream = original.WithAnotherStreamTac(name: name);
        return (original, withStream);
    }

    private static void VerifyAllStreamNames(string streamName, IDataSourceLink[] links)
    {
        foreach (var link in links)
        {
            Equal(streamName, link.OutNameTac());
            Equal(streamName, link.InNameTac());
        }
    }

    [Fact]
    public void WithAnotherStreamNoNameIsUnchanged()
    {
        var (original, withStream) = GetWithAnotherStream("");
        VerifyAllStreamNames(StreamDefaultName, [original, withStream]);
        
        Equal(original, withStream);
    }

    [Fact]
    public void WithAnotherStreamKeepsPrimaryName()
    {
        var (original, withStream) = GetWithAnotherStream();
        VerifyAllStreamNames(StreamDefaultName, [original, withStream]);
    }

    [Fact]
    public void WithAnotherStreamOnlyInName()
    {
        var original = GetWithMockDataSource();
        var withStream = original.WithAnotherStreamTac(inName: Rename);
        VerifyAllStreamNames(StreamDefaultName, [original, withStream]);

        var firstMore = withStream.MoreTac().First();
        Equal(Rename, firstMore.InNameTac());
        Equal(StreamDefaultName, firstMore.OutNameTac());
    }

    [Fact]
    public void WithAnotherStreamOnlyOutName()
    {
        var original = GetWithMockDataSource();
        var withStream = original.WithAnotherStreamTac(outName: Rename);
        VerifyAllStreamNames(StreamDefaultName, [original, withStream]);

        var firstMore = withStream.MoreTac().First();
        Equal(Rename, firstMore.OutNameTac());
        Equal(StreamDefaultName, firstMore.InNameTac());
    }

    [Fact]
    public void WithAnotherStreamLinkIsNewInstance()
    {
        var (original, withStream) = GetWithAnotherStream();

        NotNull(withStream);
        NotEqual(original, withStream);
    }

    [Fact]
    public void WithAnotherStreamPreservesOriginalMore()
    {
        // Original still empty
        var (original, _) = GetWithAnotherStream();
        Empty(original.MoreTac());
    }

    [Fact]
    public void WithAnotherStreamCreatesNewMore()
    {
        // New has a More
        var (_, withStream) = GetWithAnotherStream();
        Single(withStream.MoreTac());
    }

    [Fact]
    public void WithAnotherStreamFirstHasExpectedName()
    {
        var (_, withStream) = GetWithAnotherStream();
        VerifyAllStreamNames(Rename, [withStream.MoreTac().First()]);
    }

    [Fact]
    public void WithAnotherStreamMultiple()
    {
        var (_, withStream) = GetWithAnotherStream();
        var withAnother2 = withStream
            .WithAnotherStreamTac(name: Rename2)
            .WithAnotherStreamTac(name: Rename3);

        var more = withAnother2.MoreTac().ToList();
        
        // Should now have 3 links in the More collection
        Equal(3, more.Count);

        // The collection contains the links in latest-first order, so the first one is Rename3, then Rename2, then Rename
        VerifyAllStreamNames(Rename3, [more.First()]);

        VerifyAllStreamNames(Rename2, [more.Skip(1).First()]);

        VerifyAllStreamNames(Rename, [more.Skip(2).First()]);
    }

    [Fact]
    public void WithMore()
    {
        var original = GetWithMockDataSource();
        var withMore = original.WithMoreTac([GetWithMockDataSource("More1"), GetWithMockDataSource("More2")]);
        NotEqual(original, withMore);
        Equal(2, withMore.MoreTac().Count());
    }
}
