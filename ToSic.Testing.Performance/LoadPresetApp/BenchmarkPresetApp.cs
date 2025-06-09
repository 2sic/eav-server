using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Engines;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.DI;
using ToSic.Sys.Performance;

namespace ToSic.Testing.Performance.LoadPresetApp;

[SimpleJob(RunStrategy.Throughput, warmupCount: 1, launchCount: 1, iterationCount: 5, invocationCount: 5)]
public class BenchmarkPresetApp
{
    public const bool RunGenerateJson = Program.RunGenerateJson;
    public const int RunLoadPresets = Program.RunLoadPresets;

    [GlobalSetup]
    public void Setup()
    {
        var serviceProvider = Program.SetupServiceProvider();
        serviceProvider.Build<DoFixtureStartup<ScenarioBasic>>();
        _tester = serviceProvider.GetRequiredService<TestLoadPresetApp>();
    }

    private TestLoadPresetApp _tester;

    [Benchmark(Description = "Run with List and Immutable Dictionary")]
    public void ListAndImmutableDictionary() => RunWithSpecs(false, false);

    [Benchmark(Description = "Run with List and Frozen Dictionary")]
    public void ListAndFrozenDictionary() => RunWithSpecs(false, true);

    [Benchmark(Description = "Run with Array and Immutable Dictionary")]
    public void ArrayAndImmutableDictionary() => RunWithSpecs(true, false);

    [Benchmark(Description = "Run with Array and Frozen Dictionary")]
    public void ArrayAndFrozenDictionary() => RunWithSpecs(true, true);

    public void RunWithSpecs(bool preferArray, bool preferFrozen)
    {
        SysPerfSettings.PreferArray = preferArray;
        SysPerfSettings.PreferFrozen = preferFrozen;
        Run(0);
    }

    private void Run(int i = 0)
    {
        var runTimer = Stopwatch.StartNew();
        var appState = _tester.Run();
        runTimer.Stop();
        Console.WriteLine($@"Run {i + 1} done. Time taken: {runTimer.Elapsed}; found: ${appState.List.Count()} items");
    }

    public void RunManyTimes(bool preferArray, bool preferFrozen)
    {
        SysPerfSettings.PreferArray = preferArray;
        SysPerfSettings.PreferFrozen = preferFrozen;
        var timer = Stopwatch.StartNew();
        for (var i = 0; i < RunLoadPresets; i++)
        {
            Run(i);
        }
        timer.Stop();
        // calculate average time
        var averageTime = TimeSpan.FromTicks(timer.Elapsed.Ticks / RunLoadPresets);
        Console.WriteLine($@"Total time for {RunLoadPresets} runs: {timer.Elapsed}; Average: {averageTime}");
    }
}
