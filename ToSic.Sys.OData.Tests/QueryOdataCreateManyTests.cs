namespace ToSic.Sys.OData.Tests;

public class QueryOdataCreateManyTests
{
    private static Dictionary<string, string> Parse(IDictionary<string, string> tokens, Func<string, string> map)
        => tokens.ToDictionary(
            pair => pair.Key,
            pair => map(pair.Value)
        );

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

    [Fact]
    public void CreateManySingleSelectedStreamFallsBackToUnprefixedSelect()
    {
        var result = QueryODataParams.CreateMany(
            v => Parse(v, value => value.Contains("[QueryString:$select]") ? "unprefixed" : ""),
            ["Authors"],
            "Authors");

        True(result.TryGetValue("Authors", out var authors));
        Equal(["unprefixed"], authors.Select);
        True(authors.AllRawTac.ContainsKey(ODataConstants.SelectParamName));
    }

    [Fact]
    public void CreateManySingleSelectedStreamKeepsExplicitPrefixedSelect()
    {
        var result = QueryODataParams.CreateMany(
            v => Parse(v, value => value.Contains("[QueryString:Authors$select]")
                ? "prefixed"
                : value.Contains("[QueryString:$select]")
                    ? "unprefixed"
                    : ""),
            ["Authors"],
            "Authors");

        True(result.TryGetValue("Authors", out var authors));
        Equal(["prefixed"], authors.Select);
        Equal("prefixed", authors.AllRawTac[ODataConstants.SelectParamName]);
    }

    [Fact]
    public void CreateManyMultipleSelectedStreamsDoNotUseUnprefixedSelectAsFallback()
    {
        var result = QueryODataParams.CreateMany(
            v => Parse(v, value => value.Contains("[QueryString:$select]") ? "unprefixed" : ""),
            ["Authors", "Books"],
            "Authors,Books");

        Empty(result["Authors"].Select);
        Empty(result["Books"].Select);
    }
}
