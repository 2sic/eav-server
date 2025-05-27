using System.Text.RegularExpressions;
using ToSic.Lib.LookUp;
using ToSic.Lib.LookUp.Engines;
using ToSic.Lib.LookUp.Sources;

namespace ToSic.Eav.LookUp.Tokens;

public class TokenReplaceTest
{
    #region Test Values

    public static TheoryData<string> ValidPureTokens =
    [
        "[Source:Key]",
        "[Source:Key|format]",
        "[QueryString:ProductId||27]",
        "[QueryString:ProductId|test {3}|27]",
        "[QueryString:Something||[Module:ModuleId]]"
    ];

    public static TheoryData<string> ValidTokensWithSubTokens =>
    [
        "[AppSettings:Owner:Name]",
        "[AppSettings:Owner:Name||Nobody]",
        "[AppSettings:Owner:Name||[AppSettings:DefaultOwner:Name||Nobody]]"
    ];


    public static TheoryData<string> ValidMixedSingleTokens =
    [
        "The param was [QueryString:Id]",
        "[QueryString:Id] was the Param",
        "The param should be [QueryString:Id] but maybe not",
        "Before [Source:Key|format] after",
        "Before [QuerString:param||fallback] after",
        "before [QueryString:value||[Default:key||[VeryDefault:VDKey]]] after text",
        @"Also another
example with multiline [But:OnlyASingle] Token
or not"
    ];

    public static TheoryData<string> ValidMixedMultiTokens =
    [
        "The param was [QueryString:Id] or it is [QueryString:Something]",
        "[QueryString:Id] was the Param or this [Source:Key] or this [Kiss:This||[Or:This]]",
        @"Should also work with Multiline [QueryString:View]
Tokens or anything that 
spreads across many things
[Like:This] or like [ThisOrThat:That]
"
    ];

    private string[] VeryComplexTemplates =
    [
        @"Select * From Users Where UserId = [QueryString:UserName||[AppSettings:DefaultUserName||0]] or UserId = [AppSettings:RootUserId] or UserId = [Parameters:TestKey:Subkey||27]
and a token with sub-token for fallback [QueryString:Id||[Module:SubId]]
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should [token:key] again"
    ];

    public static TheoryData<string> InvalidTokens =
    [
        "not a token",
        "[]",
        "[ ]",
        "[...]",
        "[:]",
        "[SourceWithoutKey]",
        "[:KeyWitohutSource]",
        "[Source With Spaces:Key]",
        "[ SourceStartingWithSpace:Key]",
        "[Source|format]",
        "[Open without close",
        "Close without open]",
        "[Open||fallback]",
        "[Source::SubkeyWithoutKey]",
        "[Source:Key||[InvalidFallback]",
        "[Source:Key||[Invalid:Fallback|]"
    ];


    //private readonly Dictionary<string, string> qsValues = new()
    //{
    //    {"Id", "27"},
    //    {"View", "Details"}
    //};

    //private readonly Dictionary<string, string> srcValues = new()
    //{
    //    {"Key", "Kabcd"},
    //    {"Value", "Whatever"},
    //    {"View", "Details"}
    //};

    //private readonly LookUpInDictionary QueryString = new("QueryString");
    //private readonly LookUpInDictionary Source = new("Source");
    //private readonly Dictionary<string, ILookUp> AllSources = new(); 

    #endregion

    #region General test objects and initializer/constructor

    private readonly Regex tokenRegEx = TokenReplace.Tokenizer;
         

    //public TokenReplaceTest()
    //{
    //    // Initialize the QueryString PropertyAccess
    //    foreach (var qsValue in qsValues)
    //        QueryString.Properties.Add(qsValue.Key,qsValue.Value);
    //    foreach (var srcValue in srcValues)
    //        Source.Properties.Add(srcValue.Key, srcValue.Value);
    //    AllSources.Add(QueryString.Name, QueryString);
    //    AllSources.Add(Source.Name, Source);
    //}

    #endregion

    #region ContainsToken tests

    [Theory]
    [MemberData(nameof(ValidPureTokens))]
    public void ValidPureTokensContainsToken(string key) =>
        True(TokenReplace.ContainsTokens(key));

    [Theory]
    [MemberData(nameof(ValidMixedSingleTokens))]
    public void ValidMixedSingleTokensContainsToken(string key) =>
        True(TokenReplace.ContainsTokens(key));

    [Theory]
    [MemberData(nameof(ValidMixedMultiTokens))]
    public void ValidMixedMultiTokensContainsToken(string key) =>
        True(TokenReplace.ContainsTokens(key));

    [Theory]
    [MemberData(nameof(ValidTokensWithSubTokens))]
    public void TokenReplace_ContainsTokenWithSubtokenNew(string token) =>
        True(TokenReplace.ContainsTokens(token));

    [Theory]
    [MemberData(nameof(InvalidTokens))]
    public void TokenReplace_DoesNotContainTokenNew(string invalidToken) =>
        False(TokenReplace.ContainsTokens(invalidToken));

    #endregion

    #region Test Simple Token Detection

    [Theory]
    [MemberData(nameof(ValidPureTokens))]
    public void TokenReplace_ValidPureTokens(string testToken)
    {
        var results = tokenRegEx.Matches(testToken);
        True(results.Count == 1, "Found too many results (" + results.Count + ") for token: " + testToken);
        True(results[0].Result("${object}").Length > 0,
            "The group is of the wrong type, ${object}, but that's empty: " + testToken);
    }

    [Theory]
    [MemberData(nameof(ValidMixedSingleTokens))]
    public void TokenReplace_ValidMixedSingleTokens2(string testToken)
    {
        var results = tokenRegEx.Matches(testToken);
        True(results.Count == 1, "Should have found 1 placeholders: " + testToken);

        // Check that it only has 1 token match
        var count = CountSpecificResultTypes(results, "${object}");
        True(count == 1, "didn't find expected amount of ${object} should have found 1, found " + count + ": " + testToken);
    }

    [Theory]
    [MemberData(nameof(ValidMixedMultiTokens))]
    public void TokenReplace_ValidMixedMultiTokens(string testToken)
    {
        var results = tokenRegEx.Matches(testToken);
        True(results.Count > 1, "Should have found more than 1 placeholders: " + testToken);

        // Check that it only has 1 token match
        var count = CountSpecificResultTypes(results, "${object}");
        True(count > 1, "didn't find expected amount of ${object} should have found 1, found " + count + ": " + testToken);
    }

    [Theory]
    [MemberData(nameof(InvalidTokens))]
    public void TokenReplace_InvalidTokens(string testToken)
    {
        var results = tokenRegEx.Matches(testToken);
        var count = CountSpecificResultTypes(results, "${object}");
        True(count == 0, "Shouldn't find object - but did: " + testToken);
    }

    #endregion

    #region Simple Replace Tests
    [Theory]
    [MemberData(nameof(ValidPureTokens))]
    public void TokenReplace_SimpleReplaceOnValidPureTokens(string token)
    {
        // since the test string contains only the token, it should leave only this token as a result
        var result = tokenRegEx.Replace(token, "12345");
        True(result == "12345", "Result should be 12345, apparently it isn't. Token was " + token + ", result was: " + result);
    }
    #endregion

    #region Complex Replace Tests with 0 or 2 recurrances

    [Fact]
    public void TokenReplace_ComplexReplace()
    {
        var original =
            @"Select * From Users Where UserId = [QueryString:UserName||[AppSettings:DefaultUserName||0]] or UserId = [AppSettings:RootUserId] or UserId = [Parameters:TestKey:Subkey||27]
and a token with sub-token for fallback [QueryString:Id||[Module:SubId]]
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should [token:key] again
Now try a token which returns a token: [AppSettings:UserNameMaybeFromUrl||Johny]";
            
        // Even without recurrence it should process the fallback-token at least once
        var expectedNoRecurrence =
            @"Select * From Users Where UserId = Daniel or UserId = -1 or UserId = 27
and a token with sub-token for fallback 4567
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should What a Token! again
Now try a token which returns a token: [QueryString:UserName||Samantha]";

        var expectedRecurrence =
            @"Select * From Users Where UserId = Daniel or UserId = -1 or UserId = 27
and a token with sub-token for fallback 4567
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should What a Token! again
Now try a token which returns a token: Daniel";

        var qs = new LookUpInDictionary("QueryString");
        qs.Properties.Add("UserName", "Daniel");
        var mod = new LookUpInDictionary("Module");
        mod.Properties.Add("SubId", "4567");
        var appS = new LookUpInDictionary("AppSettings");
        appS.Properties.Add("DefaultUserName", "Name Unknown");
        appS.Properties.Add("RootUserId", "-1");
        appS.Properties.Add("UserNameMaybeFromUrl", "[QueryString:UserName||Samantha]");
        var tok = new LookUpInDictionary("token");
        tok.Properties.Add("key", "What a Token!");
        var engine = new LookUpEngine(null, sources: [
            qs,
            mod,
            appS,
            tok
        ]);

        var tr = new TokenReplace(engine);
        var resultNoRecurrence = tr.ReplaceTokens(original);
        Equal(expectedNoRecurrence, resultNoRecurrence);//, "No Recurrence");
        var resultRecurrence = tr.ReplaceTokens(original, 2);
        Equal(expectedRecurrence, resultRecurrence); //, "With 2x looping");
    }

    #endregion


    // todo
    // test sub-tokens
    // test format
    // test clean replace

    /// <summary>
    /// Helper method to count how many times a certain type of result was returned
    /// </summary>
    /// <param name="results"></param>
    /// <param name="resultName"></param>
    /// <returns></returns>
    private int CountSpecificResultTypes(MatchCollection results, string resultName)
    {
        var count = 0;
        foreach (Match match in results)
            if (!string.IsNullOrEmpty(match.Result(resultName)))
                count++;
        return count;
    }
}