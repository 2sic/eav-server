using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys;
using ToSic.Sys.HookUp;
using ToSic.Sys.Run.Startup;

namespace ToSic.HookUp.WorkSequenceTests;

public class WorkSequenceManualTests(IServiceProvider sp)
{
    #region Work Steps

    private class Add(): Calculate(op => op with { Main = op.Main + op.Change }) { }
    private class Subtract(): Calculate(op => op with { Main = op.Main - op.Change }) { }
    private class Multiply(): Calculate(op => op with { Main = op.Main * op.Change }) { }
    private class Divide(): Calculate(op => op with { Main = op.Main / op.Change }) { }
    private class Fibonacci(): Calculate(op => new(Main: op.Main + op.Change, Change: op.Main)) { }

    #endregion

    public class Startup(): QuickStartup(services =>
    {
        services.TryAddTransient<Add>();
        services.TryAddTransient<Subtract>();
        services.TryAddTransient<Multiply>();
        services.TryAddTransient<Divide>();
        services.TryAddTransient<Fibonacci>();
        services.TryAddTransient<IWork<MathOperation>, Fibonacci>();
        services.TryAddTransient<IWork<MathOperation>, Subtract>();
        services.TryAddTransient<IWork<MathOperation>, Multiply>();
        services.TryAddTransient<IWork<MathOperation>, Divide>();
        services.TryAddTransient<IWork<MathOperation>, Add>();

        services.AddHookUp();
    });

    [Fact]
    public async Task Sequence_Manual_Add3x()
    {
        var add = new Add();
        Add[] steps = [add, add, add];
        var sequence = new WorkSequenceManual<IWork<MathOperation>, MathOperation>(steps);
        var result = await sequence.Handle(new(), new MathOperation(10, 5).ToPackage());
        Equal(25, result.Data.Main);
    }
    
    [Fact]
    public async Task Sequence_Manual_SubtractDivide()
    {
        IWork<MathOperation>[] steps = [new Subtract(), new Divide()];
        var sequence = new WorkSequenceManual<IWork<MathOperation>, MathOperation>(steps);
        var result = await sequence.Handle(new(), new MathOperation(100, 5).ToPackage());
        Equal(19, result.Data.Main);
    }

    [Fact]
    public async Task Sequence_Manual_Fibonacci10()
    {
        var fibonacci = new Fibonacci();
        IWork<MathOperation>[] steps = Enumerable.Range(0, 10).Select(_ => fibonacci).ToArray();
        var sequence = new WorkSequenceManual<IWork<MathOperation>, MathOperation>(steps);
        var result = await sequence.Handle(new(), new MathOperation(1, 1).ToPackage());
        Equal(144, result.Data.Main);
    }
    
    [Fact]
    public async Task Sequence_Manual_AddMultiply()
    {
        IWork<MathOperation>[] steps = [new Add(), new Multiply()];
        var sequence = new WorkSequenceManual<IWork<MathOperation>, MathOperation>(steps);
        var result = await sequence.Handle(new(), new MathOperation(10, 5).ToPackage());
        Equal(75, result.Data.Main);
    }
    
    [Fact]
    public async Task VerifyTestSetup_Add()
    {
        //var hookup = sp.Build<WorkSequence<IWork<MathOperation>, MathOperation>>();
        var specs = new MathOperation(4, 7);
        var add = new Add();
        var result = await add.Handle(new(), specs.ToPackage());
        Equal(11, result.Data.Main);

        var again = await add.Handle(new(), result);
        Equal(11 + 7, again.Data.Main);
    }
}
