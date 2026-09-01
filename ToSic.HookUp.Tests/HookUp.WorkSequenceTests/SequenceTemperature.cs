using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys;
using ToSic.Sys.DI;
using ToSic.Sys.HookUp;
using ToSic.Sys.Run.Startup;
using Xunit.Sdk;

namespace ToSic.HookUp.WorkSequenceTests;

public class SequenceTemperature(IServiceProvider sp)
{
    #region Work Steps

    public interface ICelsiusToFahrenheitSequence : IWork<MathOperation>;

    private abstract class C2F(Func<MathOperation, MathOperation> func, int order = 0, ResultState decision = ResultState.Default)
        : Calculate(func, decision), ICelsiusToFahrenheitSequence, IWorkSequenceOrder
    {
        public int WorkSequenceOrder { get; } = order;
    }

    private class CelsiusToFahrenheit1(): C2F(op => op with { Main = op.Main * 9 });

    private class CelsiusToFahrenheit2(): C2F(op => op with { Main = op.Main / 5 });

    /// <summary>
    /// This step would break the sequence, but we want to skip it, so we set the ResultState to Skip
    /// </summary>
    private class CelsiusToFahrenheit7Skip(): C2F(op => op with { Main = 0 }, decision: ResultState.Skip);
    
    private class CelsiusToFahrenheit8Final(): C2F(op => op with { Main = op.Main + 32 }, decision: ResultState.StopSequence);

    /// <summary>
    /// This step would break the sequence, but we want to never execute it, so we set the previous ResultState to Stop
    /// </summary>
    private class CelsiusToFahrenheit9Never(): C2F(op => op with { Main = op.Main + 157 });

    #endregion

    public class Startup(): QuickStartup(services =>
    {
        // Celsius to Fahrenheit sequence, just for testing multiple implementations of the same interface
        services.AddTransient<ICelsiusToFahrenheitSequence, CelsiusToFahrenheit1>();
        services.AddTransient<ICelsiusToFahrenheitSequence, CelsiusToFahrenheit2>();
        services.AddTransient<ICelsiusToFahrenheitSequence, CelsiusToFahrenheit7Skip>();
        services.AddTransient<ICelsiusToFahrenheitSequence, CelsiusToFahrenheit9Never>();

        // Register this later, so the sequence would be wrong if the order was not respected
        services.AddTransient<ICelsiusToFahrenheitSequence, CelsiusToFahrenheit8Final>();
        services.AddHookUp();
    });

    [Theory]
    [InlineData(100, 212)]
    [InlineData(0, 32)]
    [InlineData(-40, -40)]
    [InlineData(37, 98)]
    public async Task Convert_UsingIWorkSequence(int initial, int expected)
    {
        var sequence = sp.Build<IWorkSequence<ICelsiusToFahrenheitSequence, MathOperation>>();

        var ctx = new WorkContext();

        await RunTestSequence(sequence, ctx, initial, expected);
    }

    [Theory]
    [InlineData(100, 212)]
    [InlineData(0, 32)]
    [InlineData(-40, -40)]
    [InlineData(37, 98)]
    public async Task Convert_UsingWorkSequence_AndOptions(int initial, int expected)
    {
        var sequence = sp.Build<WorkSequenceManual<ICelsiusToFahrenheitSequence, MathOperation>>();

        var ctx = new WorkContext().With(new WorkSequenceOptions
        {
            Sort = true,
            SortUnknownFirst = false,
            SortByName = true
        });

        await RunTestSequence(sequence, ctx, initial, expected);
    }
    
    [Fact]
    public async Task Convert_UsingWorkSequence_NoOptions_Fails()
        => await ThrowsAsync<EqualException>(async () =>
        {
            var sequence = sp.Build<WorkSequenceManual<ICelsiusToFahrenheitSequence, MathOperation>>();

            var ctx = new WorkContext();

            await RunTestSequence(sequence, ctx, 100, 212);
        });

    private static async Task RunTestSequence(IWork<MathOperation> sequence, WorkContext ctx, int initial, int expected)
    {
        var specs = new MathOperation(initial, 0);

        var result = await sequence.Handle(ctx, specs.ToPackage());
        Equal(expected, result.Data.Main);
    }
}
