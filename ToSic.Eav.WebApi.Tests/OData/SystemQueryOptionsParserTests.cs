using ToSic.Eav.WebApi.Sys.Admin.OData;
using static Xunit.Assert;

namespace ToSic.Eav.WebApi.Tests.Sys.Admin.OData;

public class SystemQueryOptionsParserTests
{
    private static Uri U(string queryNoQuestion) => new($"https://example.test/app/data/BlogPost?{queryNoQuestion}");

    [Fact]
    public void Select_SimpleList()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Id,Title,Content"));
        Equal(new[] { "Id", "Title", "Content" }, r.Select);
        Null(r.Filter);
        Null(r.OrderBy);
        Null(r.Top);
        Null(r.Skip);
        Null(r.Count);
        Null(r.Expand);
        Single(r.RawAllSystem);
        Empty(r.Custom);
    }

    [Fact]
    public void Select_Missing_ReturnsEmpty()
    {
        var r = SystemQueryOptionsParser.Parse(U("$filter=Title%20eq%20'X'"));
        Empty(r.Select);
        NotNull(r.Filter);
    }

    [Fact]
    public void Select_NestedParentheses_NotSplitInside()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Id,Categories(Name,Key),Author/FullName"));
        Equal(new[] { "Id", "Categories(Name,Key)", "Author/FullName" }, r.Select);
    }

    [Fact]
    public void Select_PercentEncodedKey()
    {
        var r = SystemQueryOptionsParser.Parse(U("%24select=Id,Title"));
        Equal(new[] { "Id", "Title" }, r.Select);
        // implementation unescapes keys, so the stored key is "$select"
        True(r.RawAllSystem.ContainsKey("$select"));
    }

    [Fact]
    public void Select_CaseInsensitiveKey()
    {
        var r = SystemQueryOptionsParser.Parse(U("$SeLeCt=Id,Title"));
        Equal(new[] { "Id", "Title" }, r.Select);
    }

    [Fact]
    public void Select_PrefersDollarWhenBothDollarAndEncoded()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Id,Title&%24select=Id,Overridden"));
        // Implementation unescapes keys and last-value-wins, so the later %24select overwrites $select
        Equal(new[] { "Id", "Overridden" }, r.Select);
        // Keys are unescaped, so only one entry exists ("$select") and it contains the final value
        Single(r.RawAllSystem);
        True(r.RawAllSystem.ContainsKey("$select"));
    }

    [Fact]
    public void Expand_Parses()
    {
        var r = SystemQueryOptionsParser.Parse(U("$expand=Author,Categories"));
        Equal("Author,Categories", r.Expand);
    }

    [Fact]
    public void Filter_ParsesWithEncoding()
    {
        var r = SystemQueryOptionsParser.Parse(U("$filter=Title%20eq%20'Hello%20World'"));
        Equal("Title eq 'Hello World'", r.Filter);
    }

    [Fact]
    public void OrderBy_Parses()
    {
        var r = SystemQueryOptionsParser.Parse(U("$orderby=PublicationMoment%20desc"));
        Equal("PublicationMoment desc", r.OrderBy);
    }

    [Fact]
    public void TopSkip_ParsesValid()
    {
        var r = SystemQueryOptionsParser.Parse(U("$top=10&$skip=5"));
        Equal(10, r.Top);
        Equal(5, r.Skip);
    }

    [Fact]
    public void TopSkip_InvalidTop()
    {
        var r = SystemQueryOptionsParser.Parse(U("$top=ten&$skip=2"));
        Null(r.Top);
        Equal(2, r.Skip);
    }

    [Fact]
    public void TopSkip_NegativeNumbersAreAcceptedAsInt()
    {
        var r = SystemQueryOptionsParser.Parse(U("$top=-1&$skip=-5"));
        Equal(-1, r.Top);
        Equal(-5, r.Skip);
    }

    [Theory]
    [InlineData("$count=true", true)]
    [InlineData("$count=false", false)]
    [InlineData("$count=1", true)]
    [InlineData("$count=0", false)]
    [InlineData("$count=Yes", null)]
    public void Count_VariousForms(string fragment, bool? expected)
    {
        var r = SystemQueryOptionsParser.Parse(U(fragment));
        Equal(expected, r.Count);
    }

    [Fact]
    public void Mixed_AllOptionsTogether()
    {
        var uri = U("$select=Id,Title&$expand=Author,Categories&$filter=ShowOnStartPage%20eq%20true" +
                    "&$orderby=PublicationMoment%20desc&$top=25&$skip=50&$count=true&PageId=123&ModuleId=456");
        var r = SystemQueryOptionsParser.Parse(uri);

        Equal(new[] { "Id", "Title" }, r.Select);
        Equal("Author,Categories", r.Expand);
        Equal("ShowOnStartPage eq true", r.Filter);
        Equal("PublicationMoment desc", r.OrderBy);
        Equal(25, r.Top);
        Equal(50, r.Skip);
        True(r.Count);
        Equal("123", r.Custom["PageId"]);
        Equal("456", r.Custom["ModuleId"]);
        Equal(7, r.RawAllSystem.Count); // $select,$expand,$filter,$orderby,$top,$skip,$count
    }

    [Fact]
    public void CustomParams_AreSeparated()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Id&foo=bar&baz=qux"));
        Equal(new[] { "Id" }, r.Select);
        Equal(2, r.Custom.Count);
        True(r.Custom.ContainsKey("foo"));
        True(r.Custom.ContainsKey("baz"));
    }

    [Fact]
    public void NoSystemParams_AllCustom()
    {
        var r = SystemQueryOptionsParser.Parse(U("alpha=1&beta=2"));
        Empty(r.Select);
        Empty(r.RawAllSystem);
        Equal(2, r.Custom.Count);
    }

    [Fact]
    public void EmptyQuery()
    {
        var r = SystemQueryOptionsParser.Parse(new Uri("https://example.test/app/data/BlogPost"));
        Empty(r.Select);
        Empty(r.RawAllSystem);
        Empty(r.Custom);
    }

    [Fact]
    public void Select_TrimSegments()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select= Id , Title ,  Content "));
        Equal(new[] { "Id", "Title", "Content" }, r.Select);
    }

    [Fact]
    public void Select_WithParenthesesAndTrailingComma()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Categories(Name,Key),"));
        Equal(new[] { "Categories(Name,Key)" }, r.Select);
    }

    [Fact]
    public void Count_InvalidValue()
    {
        var r = SystemQueryOptionsParser.Parse(U("$count=maybe"));
        Null(r.Count);
    }
}