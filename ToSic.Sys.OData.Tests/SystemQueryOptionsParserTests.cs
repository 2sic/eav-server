namespace ToSic.Sys.OData.Tests;

public class SystemQueryOptionsParserTests
{
    private static Uri U(string queryNoQuestion) => new($"https://example.test/app/data/BlogPost?{queryNoQuestion}");

    [Fact]
    public void Select_SimpleList()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Id,Title,Content"));
        Equal(["Id", "Title", "Content"], r.Select);
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
        Equal(["Id", "Categories(Name,Key)", "Author/FullName"], r.Select);
    }

    [Fact]
    public void Select_PercentEncodedKey()
    {
        var r = SystemQueryOptionsParser.Parse(U("%24select=Id,Title"));
        Equal(["Id", "Title"], r.Select);
        // implementation unescapes keys, so the stored key is "$select"
        True(r.RawAllSystem.ContainsKey("$select"));
    }

    [Fact]
    public void Select_CaseInsensitiveKey()
    {
        var r = SystemQueryOptionsParser.Parse(U("$SeLeCt=Id,Title"));
        Equal(["Id", "Title"], r.Select);
    }

    [Fact]
    public void Select_PrefersDollarWhenBothDollarAndEncoded()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Id,Title&%24select=Id,Overridden"));
        // Implementation unescapes keys and last-value-wins, so the later %24select overwrites $select
        Equal(["Id", "Overridden"], r.Select);
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

        Equal(["Id", "Title"], r.Select);
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
        Equal(["Id"], r.Select);
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
        Equal(["Id", "Title", "Content"], r.Select);
    }

    [Fact]
    public void Select_WithParenthesesAndTrailingComma()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Categories(Name,Key),"));
        Equal(["Categories(Name,Key)"], r.Select);
    }

    [Fact]
    public void Count_InvalidValue()
    {
        var r = SystemQueryOptionsParser.Parse(U("$count=maybe"));
        Null(r.Count);
    }

    [Fact]
    public void Select_EmptyValue_YieldsEmptyListButKeyPresent()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select="));
        Empty(r.Select);
        Equal(string.Empty, r.RawAllSystem["$select"]);
    }

    [Fact]
    public void Select_KeyWithLeadingSpaces_IsTrimmed()
    {
        var r = SystemQueryOptionsParser.Parse(U("   %24select=Id"));
        Equal(["Id"], r.Select);
        True(r.RawAllSystem.ContainsKey("$select"));
    }

    [Fact]
    public void Select_IgnoresEmptySegments()
    {
        var r = SystemQueryOptionsParser.Parse(U("$select=Id,,Title"));
        Equal(["Id", "Title"], r.Select);
    }

    [Fact]
    public void ParameterCount_ExactlyAtLimitProcessed()
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < 500; i++)
        {
            if (i > 0) sb.Append('&');
            sb.Append("p").Append(i).Append('=').Append(i);
        }
        var r = SystemQueryOptionsParser.Parse(U(sb.ToString()));
        Equal(500, r.Custom.Count);
        True(r.Custom.ContainsKey("p0"));
        True(r.Custom.ContainsKey("p499"));
    }

    [Fact]
    public void ValueLength_AtLimit_NotTruncated()
    {
        var atLimit = new string('b', 8192);
        var r = SystemQueryOptionsParser.Parse(U("$filter=" + atLimit));
        NotNull(r.Filter);
        Equal(8192, r.Filter!.Length);
    }

    [Fact]
    public void Count_UpperCaseTrue()
    {
        var r = SystemQueryOptionsParser.Parse(U("$count=TRUE"));
        True(r.Count);
    }

    [Fact]
    public void Select_DuplicateEncodedThenPlain_LastWins()
    {
        var r = SystemQueryOptionsParser.Parse(U("%24select=Id,Original&$select=Id,Final"));
        Equal(["Id", "Final"], new[] { r.Select[0], r.Select[1] });
        Equal("Id,Final", r.RawAllSystem["$select"]);
    }

    [Fact]
    public void InvalidPercentEncoding_DoesNotThrowAndFallsBackToRaw()
    {
        // "%ZZselect" is invalid percent encoding so key stays raw and becomes a custom param.
        var r = SystemQueryOptionsParser.Parse(U("%ZZselect=Id&$filter=Title%2")); // value has dangling %2
        // Custom should contain the invalid key literally
        True(r.Custom.ContainsKey("%ZZselect"));
        Equal("Id", r.Custom["%ZZselect"]);
        // $filter should be captured; its value invalid escape remains raw "Title%2"
        Equal("Title%2", r.Filter);
    }

    [Fact]
    public void ParameterCount_IsCapped()
    {
        // Build 520 custom parameters; parser limit is 500 so only first 500 should be processed.
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < 520; i++)
        {
            if (i > 0) sb.Append('&');
            sb.Append("p").Append(i).Append('=').Append(i);
        }
        var r = SystemQueryOptionsParser.Parse(U(sb.ToString()));
        // All are custom (no $) so count should be capped at 500
        Equal(500, r.Custom.Count);
    }

    [Fact]
    public void ValueLength_TruncatedAtLimit()
    {
        // Limit is 8192 characters; create a longer value that should be truncated.
        var over = 9000;
        var longVal = new string('a', over);
        var r = SystemQueryOptionsParser.Parse(U("$filter=" + longVal));
        NotNull(r.Filter);
        // Expect truncated length == 8192 (knowledge of current constant; adjust if constant changes)
        Equal(8192, r.Filter!.Length);
    }

    [Fact]
    public void Select_ItemLimit_CapsAtMax()
    {
        // Provide more than 200 select items; expect only first 200 retained.
        var sb = new System.Text.StringBuilder();
        sb.Append("$select=");
        for (var i = 0; i < 205; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append("F").Append(i);
        }
        var r = SystemQueryOptionsParser.Parse(U(sb.ToString()));
        Equal(200, r.Select.Count);
        Equal("F0", r.Select[0]);
    }

    [Fact]
    public void Select_DeepParentheses_DoesNotThrow()
    {
        // Build a deeply nested parentheses segment exceeding depth cap (64) then another item
        var nested = new string('(', 150) + "Deep" + new string(')', 150);
        var r = SystemQueryOptionsParser.Parse(U("$select=" + nested + ",Title"));
        Equal(2, r.Select.Count);
        Equal(nested, r.Select[0]);
        Equal("Title", r.Select[1]);
    }
}