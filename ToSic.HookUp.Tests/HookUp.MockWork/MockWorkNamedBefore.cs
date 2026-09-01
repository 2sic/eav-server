using ToSic.Sys.HookUp;

namespace ToSic.HookUp.MockWork;

internal class MockWorkNamedBefore: MockWorkStringAddBase
{
    public const string PhaseName = "RunBefore";
    public const string AddOn = "Run before";

    public override string Add => AddOn;
}