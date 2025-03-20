namespace ToSic.Eav.Plumbing;

public class SubStringTests
{
    [Fact]
    public void AfterBasic() => Equal("result", "before stuff and something=result".AfterTac("something="));

    [Fact]
    public void AfterBasicCaseInsensitive() => Equal("result", "before stuff and something=result".AfterTac("SOMETHING="));

    [Fact]
    public void AfterBasicCaseSensitive() => Equal(null, "before stuff and something=result".AfterTac("SOMETHING=", true));

    [Fact]
    public void AfterNotFound() => Equal(null, "before stuff and something=result".AfterTac("doesn't exist"));

    [Fact]
    public void AfterNotFoundWithDefined() => Equal("", "before stuff and something=result".AfterTac("doesn't exist") ?? "");

    [Fact]
    public void AfterAtEnd() => Equal("", "before stuff and something=result".AfterTac("result"));

    [Fact]
    public void AfterWithNullValue() => Equal(null, ((string)null).AfterTac("result"));
        
    [Fact]
    public void AfterWithNullKeyAndValue() => Equal(null, ((string)null).AfterTac(null));

    [Fact]
    public void AfterWithNullKey() => Equal(null, "something".AfterTac(null));

    [Fact]
    public void AfterMultiple() => Equal("y x=7 x=99", "before stuff and x=y x=7 x=99".AfterTac("x="));


    // BETWEEN
    [Fact] public void BetweenBasic() => Equal(" stuff ", "before stuff and something=result".BetweenTac("before", "and"));
    [Fact] public void BetweenNull() => Equal(null, ((string)null).BetweenTac("before", "and"));
    [Fact] public void BetweenStartNotFound() => Equal(null, "before stuff and something=result".BetweenTac("don't exist", "and"));
    [Fact] public void BetweenEndNotFound() => Equal(null, "before stuff and something=result".BetweenTac("before", "not found"));
    [Fact] public void BetweenEndNotFoundToEnd() => Equal(" stuff and something=result", "before stuff and something=result".BetweenTac("before", "not found", true));

}