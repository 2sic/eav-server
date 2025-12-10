using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys;
using ToSic.Eav.LookUp.Sys.Engines;
#pragma warning disable xUnit1045

namespace ToSic.Sys.LookUp.Tokens;

/// <summary>
/// Test what happens if the fallback has multiple separate tokens - doesn't seem to work till v20.09
/// </summary>
public class TokenReplaceTestMultipleFallbacks
{
    #region Arrange: Data

    public static TheoryData<TokenTest> MultipleFallbackTokens =>
    [
        new("[Org:CustomRange||[Org:Start]-[Org:End]]"),
    ];


    #endregion

    #region Arrange: Setup TokenReplace

    [field: AllowNull, MaybeNull]
    private TokenReplace TokRep => field ??= CreateTokenReplaceWithTestSources();

    private static TokenReplace CreateTokenReplaceWithTestSources()
    {
        var qs = new LookUpInDictionary("Org");
        qs.Properties.Add("CustomRange", "");
        qs.Properties.Add("Start", "2021");
        qs.Properties.Add("End", "2025");
        var engine = new LookUpEngine(null, sources: [qs]);
        return new(engine);
    }

    #endregion

    #region Act, Assert

    [Theory, MemberData(nameof(MultipleFallbackTokens))]
    public void NoRecursions(TokenTest test)
    {
        var result = TokRep.ReplaceTokens(test.Template);
        True("2021-2025" == result, $"result: {result}\n{test}");
    }

    #endregion

}