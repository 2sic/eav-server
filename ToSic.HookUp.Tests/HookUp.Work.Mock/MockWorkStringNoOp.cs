using ToSic.Sys.HookUp;

namespace ToSic.HookUp.Work.Mock;

internal class MockWorkStringNoOp: IWork<string, string>
{
    public Task<Package<string>> Handle(WorkContext mainCtx, Package<string> package)
        => Task.FromResult(package);
}

internal class MockWorkStringAddWorld : IWork<string, string>
{
    public Task<Package<string>> Handle(WorkContext mainCtx, Package<string> package)
        => Task.FromResult((package.Data + " World").ToPackage());
}

internal class MockWorkStringNoOpWithException : IWork<string, string?>
{
    public const string ErrorMessage = "Mock exception for testing purposes.";
    
    public Task<Package<string?>> Handle(WorkContext mainCtx, Package<string> package)
        => Task.FromResult(new Package<string?>
        {
            Data = null,
            Decision = DataPreprocessorDecision.Error,
            Exceptions = [new(ErrorMessage)]
        });
}
