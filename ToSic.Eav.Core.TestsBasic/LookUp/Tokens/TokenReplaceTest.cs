using System.Text.RegularExpressions;
using ToSic.Eav.LookUp.Sys;
#pragma warning disable xUnit1045

namespace ToSic.Sys.LookUp.Tokens;

public class TokenReplaceTest
{
    // todo
    // test sub-tokens
    // test format
    // test clean replace

    #region Test Values

    public static TheoryData<TokenTest> ValidPureTokens =
    [
        new("[Source:Key]", "basic token"),
        new("[Source:Key|format]", "basic token w/format"),
        new("[QueryString:ProductId||27]", "basic token with static fallback"),
        new("[QueryString:ProductId|test {3}|27]", "basic token with format and static fallback"),
        new("[QueryString:Something||[Module:ModuleId]]", "basic token with fallback token"),
    ];

    public static TheoryData<TokenTest> ValidTokensWithSubTokens =>
    [
        new("[AppSettings:Owner:Name]"),
        new("[AppSettings:Owner:Name||Nobody]"),
        new("[AppSettings:Owner:Name||[AppSettings:DefaultOwner:Name||Nobody]]"),
    ];


    public static TheoryData<TokenTest> ValidMixedSingleTokens =
    [
        new("The param was [QueryString:Id]"),
        new("[QueryString:Id] was the Param"),
        new("The param should be [QueryString:Id] but maybe not"),
        new("Before [Source:Key|format] after"),
        new("Before [QueryString:param||fallback] after"),
        new("before [QueryString:value||[Default:key||[VeryDefault:VDKey]]] after text"),
        new("""
            Also another
            example with multiline [But:OnlyASingle] Token
            or not
            """),
    ];

    public static TheoryData<TokenTest> ValidMixedMultiTokens =
    [
        new("The param was [QueryString:Id] or it is [QueryString:Something]"),
        new("[QueryString:Id] was the Param or this [Source:Key] or this [Kiss:This||[Or:This]]"),
        new("""
            Should also work with Multiline [QueryString:View]
            Tokens or anything that 
            spreads across many things
            [Like:This] or like [ThisOrThat:That]

            """),
    ];

    #endregion

    #region General test objects and initializer/constructor

    [field: AllowNull, MaybeNull]
    private Regex TokenRegEx => field ??= TokenReplace.Tokenizer;
         

    #endregion

    #region ContainsToken tests

    [Theory, MemberData(nameof(ValidPureTokens))]
    public void ValidPureTokensContainsToken(TokenTest test) =>
        True(TokenReplace.ContainsTokens(test.Template));

    [Theory, MemberData(nameof(ValidMixedSingleTokens))]
    public void ValidMixedSingleTokensContainsToken(TokenTest test) =>
        True(TokenReplace.ContainsTokens(test.Template));

    [Theory, MemberData(nameof(ValidMixedMultiTokens))]
    public void ValidMixedMultiTokensContainsToken(TokenTest test) =>
        True(TokenReplace.ContainsTokens(test.Template));

    [Theory, MemberData(nameof(ValidTokensWithSubTokens))]
    public void TokenReplace_ContainsTokenWithSubTokenNew(TokenTest test) =>
        True(TokenReplace.ContainsTokens(test.Template));

    #endregion

    #region Test Simple Token Detection

    [Theory, MemberData(nameof(ValidPureTokens))]
    public void TokenReplace_ValidPureTokens(TokenTest testToken)
    {
        var results = TokenRegEx.Matches(testToken.Template);
        True(results.Count == 1, "Found too many results (" + results.Count + ") for token: " + testToken);
        True(results[0].Result("${object}").Length > 0,
            "The group is of the wrong type, ${object}, but that's empty: " + testToken);
    }

    [Theory, MemberData(nameof(ValidMixedSingleTokens))]
    public void TokenReplace_ValidMixedSingleTokens2(TokenTest testToken)
    {
        var results = TokenRegEx.Matches(testToken.Template);
        True(results.Count == 1, "Should have found 1 placeholders: " + testToken);

        // Check that it only has 1 token match
        var count = results.CountSpecificResultTypes("${object}");
        True(count == 1, "didn't find expected amount of ${object} should have found 1, found " + count + ": " + testToken);
    }

    [Theory, MemberData(nameof(ValidMixedMultiTokens))]
    public void TokenReplace_ValidMixedMultiTokens(TokenTest testToken)
    {
        var results = TokenRegEx.Matches(testToken.Template);
        True(results.Count > 1, "Should have found more than 1 placeholders: " + testToken);

        // Check that it only has 1 token match
        var count = results.CountSpecificResultTypes("${object}");
        True(count > 1, "didn't find expected amount of ${object} should have found 1, found " + count + ": " + testToken);
    }

    #endregion

    #region Simple Replace Tests

    [Theory, MemberData(nameof(ValidPureTokens))]
    public void TokenReplace_SimpleReplaceOnValidPureTokens(TokenTest token)
    {
        // since the test string contains only the token, it should leave only this token as a result
        var result = TokenRegEx.Replace(token.Template, "12345");
        True(result == "12345", "Result should be 12345, apparently it isn't. Token was " + token + ", result was: " + result);
    }
    #endregion



}