using ToSic.Sys.HookUp;

namespace ToSic.HookUp.MockWork;

public abstract class MockWorkStringAddBase: IWork<string, string>
{
    public abstract string Add { get; }

    public Task<Package<string>> Handle(WorkContext mainCtx, Package<string> package)
        => Task.FromResult((package.Data + Add).ToPackage());
}