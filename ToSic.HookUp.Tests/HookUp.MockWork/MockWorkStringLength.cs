using ToSic.Sys.HookUp;

namespace ToSic.HookUp.MockWork;

public class MockWorkStringLength: IWork<string, int>
{
    public Task<Package<int>> Handle(WorkContext mainCtx, Package<string> package)
        => Task.FromResult(package.Data.Length.ToPackage());
}