namespace ToSic.HookUp.MockWork;

internal class MockWorkNamedAfter : MockWorkStringAddBase
{
    public const string PhaseName = "RunAfter";
    public const string AddOn = "Run after";

    public override string Add => AddOn;
}