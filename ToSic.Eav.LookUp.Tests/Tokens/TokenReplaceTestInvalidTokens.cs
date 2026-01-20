using System.Text.RegularExpressions;
using ToSic.Eav.LookUp.Sys;
#pragma warning disable xUnit1045

namespace ToSic.Sys.LookUp.Tokens;

public class TokenReplaceTestInvalidTokens
{
    #region Test Values

    public static TheoryData<TokenTest> InvalidTokens =
    [
        new("not a token"),
        new("[]"),
        new("[ ]"),
        new("[...]"),
        new("[:]"),
        new("[SourceWithoutKey]"),
        new("[:KeyWithoutSource]"),
        new("[Source With Spaces:Key]"),
        new("[ SourceStartingWithSpace:Key]"),
        new("[Source|format]"),
        new("[Open without close"),
        new("Close without open]"),
        new("[Open||fallback]"),
        new("[Source::SubKeyWithoutKey]"),
        new("[Source:Key||[InvalidFallback]"),
        new("[Source:Key||[Invalid:Fallback|]")
    ];

    #endregion

    #region General test objects and initializer/constructor

    [field: AllowNull, MaybeNull]
    private Regex TokenRegEx => field ??= TokenReplace.Tokenizer;

    #endregion

    #region ContainsToken tests


    [Theory, MemberData(nameof(InvalidTokens))]
    public void DoesNotContainToken(TokenTest invalidToken) =>
        False(TokenReplace.ContainsTokens(invalidToken.Template));

    #endregion

    #region Test Simple Token Detection

    [Theory, MemberData(nameof(InvalidTokens))]
    public void Replace(TokenTest testToken)
    {
        var results = TokenRegEx.Matches(testToken.Template);
        var count = results.CountSpecificResultTypes("${object}");
        True(count == 0, "Shouldn't find object - but did: " + testToken);
    }

    #endregion

}