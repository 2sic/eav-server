namespace ToSic.Sys.OData.Tests;

public class QueryOdataCreateManyTests
{
    private static Dictionary<string, string> Parse(IDictionary<string, string> tokens, Func<string, string> map)
        => tokens.ToDictionary(
            pair => pair.Key,
            pair => map(pair.Value)
        );

    private static Dictionary<string, string> ParseExact(IDictionary<string, string> tokens, IReadOnlyDictionary<string, string> values)
        => tokens.ToDictionary(
            pair => pair.Key,
            pair => values.TryGetValue(pair.Value, out var value) ? value : ""
        );

    [Theory]
    [InlineData]
    [InlineData("Default")]
    public void CreateManyDefault(params string[] names)
    {
        var x = QueryODataParamsTestAccessors.CreateManyTac(v => v, names);
        NotNull(x);
        Single(x);
        Equal("Default", x.First().Key);
    }

    [Theory]
    [InlineData(2, "Default", "More")]
    public void CreateManyDefaultAndMore(int count, params string[] names)
    {
        var x = QueryODataParamsTestAccessors.CreateManyTac(v => v, names);
        NotNull(x);
        Equal(count, x.Count);
        Equal(names, x.Select(pair => pair.Key).ToArray());
    }

    [Fact]
    public void CreateManySingleSelectedStreamFallsBackToUnprefixedSelect()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => Parse(v, value => value.Contains("[QueryString:$select]") ? "unprefixed" : ""),
            ["Authors"],
            "Authors");

        True(result.TryGetValue("Authors", out var authors));
        Equal(["unprefixed"], authors.SelectTac);
        True(authors.AllRawTac.ContainsKey(ODataConstants.SelectParamName));
    }

    [Fact]
    public void CreateManySingleSelectedStreamKeepsExplicitPrefixedSelect()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => Parse(v, value => value.Contains("[QueryString:Authors$select]")
                ? "prefixed"
                : value.Contains("[QueryString:$select]")
                    ? "unprefixed"
                    : ""),
            ["Authors"],
            "Authors");

        True(result.TryGetValue("Authors", out var authors));
        Equal(["prefixed"], authors.SelectTac);
        Equal("prefixed", authors.AllRawTac[ODataConstants.SelectParamName]);
    }

    [Fact]
    public void CreateManySingleSelectedStreamFallsBackToUnprefixedFilter()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => ParseExact(v, new Dictionary<string, string>
            {
                ["[QueryString:$filter]"] = "LastName eq 'Adams'"
            }),
            ["Authors"],
            "Authors");

        var authors = result["Authors"];
        Equal("LastName eq 'Adams'", authors.FilterTac);
        Equal("LastName eq 'Adams'", authors.AllRawTac[ODataConstants.FilterParamName]);
    }

    [Fact]
    public void CreateManySingleSelectedStreamFallsBackToUnprefixedOrderBy()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => ParseExact(v, new Dictionary<string, string>
            {
                ["[QueryString:$orderby]"] = "LastName desc"
            }),
            ["Authors"],
            "Authors");

        var authors = result["Authors"];
        Equal("LastName desc", authors.OrderByTac);
        Equal("LastName desc", authors.AllRawTac[ODataConstants.OrderByParamName]);
    }

    [Fact]
    public void CreateManySingleSelectedStreamFallsBackToUnprefixedTopAndSkip()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => ParseExact(v, new Dictionary<string, string>
            {
                ["[QueryString:$top]"] = "5",
                ["[QueryString:$skip]"] = "10"
            }),
            ["Authors"],
            "Authors");

        var authors = result["Authors"];
        Equal(5, authors.TopTac);
        Equal(10, authors.SkipTac);
        Equal("5", authors.AllRawTac[ODataConstants.TopParamName]);
        Equal("10", authors.AllRawTac[ODataConstants.SkipParamName]);
    }

    [Fact]
    public void CreateManySingleSelectedStreamMergesPerParameterAndKeepsPrefixedPrecedence()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => ParseExact(v, new Dictionary<string, string>
            {
                ["[QueryString:$filter]"] = "bare-filter",
                ["[QueryString:$orderby]"] = "bare-order",
                ["[QueryString:Authors$filter]"] = "prefixed-filter"
            }),
            ["Authors"],
            "Authors");

        var authors = result["Authors"];
        Equal("prefixed-filter", authors.FilterTac);
        Equal("bare-order", authors.OrderByTac);
        Equal("prefixed-filter", authors.AllRawTac[ODataConstants.FilterParamName]);
        Equal("bare-order", authors.AllRawTac[ODataConstants.OrderByParamName]);
    }

    [Fact]
    public void CreateManyMultipleSelectedStreamsDoNotUseUnprefixedSelectAsFallback()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => Parse(v, value => value.Contains("[QueryString:$select]") ? "unprefixed" : ""),
            ["Authors", "Books"],
            "Authors,Books");

        Empty(result["Authors"].SelectTac);
        Empty(result["Books"].SelectTac);
    }

    [Fact]
    public void CreateManyMultipleSelectedStreamsDoNotUseUnprefixedFilterAsFallback()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => ParseExact(v, new Dictionary<string, string>
            {
                ["[QueryString:$filter]"] = "LastName eq 'Adams'"
            }),
            ["Authors", "Books"],
            "Authors,Books");

        Null(result["Authors"].FilterTac);
        Null(result["Books"].FilterTac);
    }

    [Fact]
    public void CreateManySingleSelectedStreamWithBareFilterIsNotEmptyExceptForSelect()
    {
        var result = QueryODataParamsTestAccessors.CreateManyTac(
            v => ParseExact(v, new Dictionary<string, string>
            {
                ["[QueryString:$filter]"] = "LastName eq 'Adams'"
            }),
            ["Authors"],
            "Authors");

        False(result["Authors"].IsEmptyExceptForSelect());
    }
}
