using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ToSic.Eav.Data;

namespace ToSic.Testing.Performance.LazyFastAccess;

[SimpleJob(RunStrategy.Throughput, warmupCount: 1, launchCount: 1, iterationCount: 5, invocationCount: 2)]
public class BenchmarkLazyFastAccessOfType
{
    public const int Runs = 100;

    [GlobalSetup]
    public void Setup()
    {
        var data = LazyFastAccessTestHelper.LoadAppAndGetList();
        List = data.List;

        Types = data.AppState.ContentTypes.ToListOpt();
    }


    private IEnumerable<IEntity> List { get; set; }

    private IList<IContentType> Types { get; set; }


    [Benchmark(Description = "Run OfType(string) without Lazy-Fast-Access")]
    public void GetIntWithoutLazyFastAccess() => RunWithSpecs(false, Runs);

    [Benchmark(Description = "Run OfType(string) with Lazy-Fast-Access")]
    public void GetIntWithLazyFastAccess() => RunWithSpecs(true, Runs);

    [Benchmark(Description = "Run OfType(string) with Lazy-Fast-Access, MUTABLE")]
    public void GetIntWithLazyFastAccessMutable() => RunWithSpecsCol(true, Runs);

    public void RunWithSpecs(bool enableLfa, int runs)
    {
        Console.WriteLine($@"Starting with {Types.Count} IDs, App has {List.Count()} items; App has {Types.Count} Types");
        SysPerfSettings.CacheListAutoIndex = enableLfa;
        var totalTime = new TimeSpan();
        for (var i = 0; i < Runs; i++)
        {
            var ts = Run(enableLfa, i);
            totalTime += ts;
        }
        Console.WriteLine($@"Total time for {Runs} runs with LFA={enableLfa}: {totalTime}");
    }
    public void RunWithSpecsCol(bool enableLfa, int runs)
    {
        Console.WriteLine($@"Starting with {Types.Count} IDs, App has {List.Count()} items; App has {Types.Count} Types");
        SysPerfSettings.CacheListAutoIndex = enableLfa;
        var totalTime = new TimeSpan();
        for (var i = 0; i < Runs; i++)
        {
            var ts = RunCol(enableLfa, i);
            totalTime += ts;
        }
        Console.WriteLine($@"Total time for {Runs} runs with LFA={enableLfa}: {totalTime}");
    }

    private TimeSpan Run(bool enableLfa, int i = 0)
    {
        var runTimer = new Stopwatch();
        foreach (var contentType in Types)
        {
            runTimer.Start();
            var found = List.OfType(contentType.Name);
            runTimer.Stop();
        }

        return runTimer.Elapsed;
    }
    private TimeSpan RunCol(bool enableLfa, int i = 0)
    {
        var runTimer = new Stopwatch();
        foreach (var contentType in Types)
        {
            runTimer.Start();
            var found = List.OfTypeCol(contentType.Name);
            runTimer.Stop();
            // Console.WriteLine($@"Type: {contentType.Name} found: ${found.Count()} items");
        }

        //Console.WriteLine($@"Run {i + 1} done; LFA: {enableLfa}. Time taken: {runTimer.Elapsed};");
        return runTimer.Elapsed;
    }
}
