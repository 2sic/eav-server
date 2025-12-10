using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys;
using ToSic.Eav.LookUp.Sys.Engines;

namespace ToSic.Sys.LookUp.Tokens;

/// <summary>
/// Test different behaviors of TokenReplace with and without recursions
/// </summary>
public class TokenReplaceTestRecursions
{
    #region Arrange: Data

    private const string Template =
        """
        Select * From Users Where UserId = [QueryString:UserName||[AppSettings:DefaultUserName||0]] or UserId = [AppSettings:RootUserId] or UserId = [Parameters:TestKey:Subkey||27]
        and a token with sub-token for fallback [QueryString:Id||[Module:SubId]]
        some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

        and a [bad token without property] and a [Source::SubkeyWithoutKey]
        but this should [token:key] again
        Now try a token which returns a token: [AppSettings:UserNameMaybeFromUrl||Johny]
        """;

    // Even without recursion it should process the fallback-token at least once
    private const string ExpectedNoRecursion =
        """
        Select * From Users Where UserId = Daniel or UserId = -1 or UserId = 27
        and a token with sub-token for fallback 4567
        some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

        and a [bad token without property] and a [Source::SubkeyWithoutKey]
        but this should What a Token! again
        Now try a token which returns a token: [QueryString:UserName||Samantha]
        """;
    
    private const string ExpectedRecursion =
        """
        Select * From Users Where UserId = Daniel or UserId = -1 or UserId = 27
        and a token with sub-token for fallback 4567
        some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

        and a [bad token without property] and a [Source::SubkeyWithoutKey]
        but this should What a Token! again
        Now try a token which returns a token: Daniel
        """;

    #endregion

    #region Arrange: Setup TokenReplace

    [field: AllowNull, MaybeNull]
    private TokenReplace TokRep => field ??= CreateTokenReplaceWithTestSources();

    private static TokenReplace CreateTokenReplaceWithTestSources()
    {
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

        return new(engine);
    }

    #endregion

    #region Act, Assert

    [Fact]
    public void NoRecursions()
        => Equal(ExpectedNoRecursion, TokRep.ReplaceTokens(Template));

    [Fact]
    public void WithRecursions2()
        => Equal(ExpectedRecursion, TokRep.ReplaceTokens(Template, 2));

    #endregion

}