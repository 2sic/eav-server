namespace ToSic.Sys.OData.Tests;

public class QueryOdataCreateManyTests
{
    [Theory]
    [InlineData]
    [InlineData("Default")]
    public void CreateManyDefault(params string[] names)
    {
        var x = QueryODataParams.CreateMany(v => v, names);
        NotNull(x);
        Single(x);
        Equal("Default", x.First().Key);
    }

    [Theory]
    [InlineData(2, "Default", "More")]
    public void CreateManyDefaultAndMore(int count, params string[] names)
    {
        var x = QueryODataParams.CreateMany(v => v, names);
        NotNull(x);
        Equal(count, x.Count);
        Equal(names, x.Select(pair => pair.Key).ToArray());
    }
}
