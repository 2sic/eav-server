using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Entities.Sys.Lists;

namespace ToSic.Testing.Performance.LazyFastAccess;

[SimpleJob(RunStrategy.Throughput, warmupCount: 1, launchCount: 1, iterationCount: 5, invocationCount: 10)]
public class BenchmarkLazyFastAccessGetInt
{
    public int IdsToFind = 100;
    public const int Runs = 100;

    [GlobalSetup]
    public void Setup() => List = LazyFastAccessTestHelper.LoadAppAndGetList().List;


    private IEnumerable<IEntity> List { get; set; }

    private IList<int> Ids => field ??= List
        .Reverse()
        .Take(IdsToFind)
        .Select(e => e.EntityId)
        .ToListOpt();

    [Benchmark(Description = "Run Get(int) without Lazy-Fast-Access")]
    public void GetIntWithoutLazyFastAccess() => RunWithSpecs(false, Runs);

    [Benchmark(Description = "Run Get(int) with Lazy-Fast-Access")]
    public void GetIntWithLazyFastAccess() => RunWithSpecs(true, Runs);

    public void RunWithSpecs(bool enableLfa, int runs)
    {
        Console.WriteLine($@"Starting with {Ids.Count} IDs, App has {List.Count()} items");
        SysPerfSettings.EnableLazyFastAccess = enableLfa;
        var totalTime = new TimeSpan();
        for (var i = 0; i < Runs; i++)
        {
            var ts = Run(enableLfa, i);
            totalTime += ts;
        }
        Console.WriteLine($@"Total time for {Runs} runs with LFA={enableLfa}: {totalTime}");
    }

    private TimeSpan Run(bool enableLfa, int i = 0)
    {
        var runTimer = Stopwatch.StartNew();
        var foundCount = Ids
            .Select(id => List.One(id))
            .Count(found => found != null);

        runTimer.Stop();
        Console.WriteLine($@"Run {i + 1} done; LFA: {enableLfa}. Time taken: {runTimer.Elapsed}; found: ${foundCount} items; expected {IdsToFind}");
        return runTimer.Elapsed;
    }
}
