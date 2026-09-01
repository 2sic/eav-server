using ToSic.Sys.HookUp;

namespace ToSic.HookUp.WorkSequenceTests;

public abstract class Calculate(Func<MathOperation, MathOperation> func, ResultState decision = ResultState.Default): IWork<MathOperation>
{
    public Task<Package<MathOperation>> Handle(WorkContext mainCtx, Package<MathOperation> package)
    {
        var updated = package with
        {
            Data = Calc(package.Data),
            Decision = Decision
        };
        return Task.FromResult(updated);
    }
        
    protected virtual MathOperation Calc(MathOperation op) => func(op);
    protected virtual ResultState Decision => decision;
}
