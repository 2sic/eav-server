using ToSic.Sys.HookUp;

namespace ToSic.HookUp.Work.DoNamed;

internal class MockNamedBefore: IWork<string, string>
{
    public const string PhaseName = "RunBefore";
    
    public Task<Package<string>> Handle(WorkContext mainCtx, Package<string> package)
        => Task.FromResult("Run before".ToPackage());
}

internal class MockNamedAfter : IWork<string, string>
{
    public const string PhaseName = "RunAfter";

    public Task<Package<string>> Handle(WorkContext mainCtx, Package<string> package)
        => Task.FromResult("Run After".ToPackage());
}