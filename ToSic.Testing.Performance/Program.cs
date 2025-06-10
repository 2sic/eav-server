using System;
using System.Diagnostics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Testing.Performance.json;
using ToSic.Testing.Performance.LazyFastAccess;
using ToSic.Testing.Performance.LoadPresetApp;

namespace ToSic.Testing.Performance;

class Program
{
    public const bool RunGenerateJson = false;
    public const int RunLoadPresets = 25;

    public static bool ModeBenchmark = false;

    public static PerformanceTestTypes CurrentTest = PerformanceTestTypes.LazyFastAccessOfType;

    static void Main(string[] args)
    {
        // When running in benchmark mode, we run the benchmarks and nothing else
        if (ModeBenchmark)
        {
            Summary summary = null;
            switch (CurrentTest)
            {
                case PerformanceTestTypes.LazyFastAccessGetInt:
                    summary = BenchmarkRunner.Run<BenchmarkLazyFastAccessGetInt>();
                    return;
                case PerformanceTestTypes.LazyFastAccessOfType:
                    summary = BenchmarkRunner.Run<BenchmarkLazyFastAccessOfType>();
                    return;
                case PerformanceTestTypes.GenerateJson:
                    // No benchmark for this, as it's just a json generation test
                    return;
                case PerformanceTestTypes.LoadPresetApp:
                    // Benchmark for loading preset app
                    summary = BenchmarkRunner.Run<BenchmarkPresetApp>();
                    break;
            }
            Console.WriteLine(summary?.ToString());
        }
        // Otherwise - usually when running with the performance profiler - we run the tests
        else
        {

            switch (CurrentTest)
            {
                case PerformanceTestTypes.LazyFastAccessGetInt:
                    var lfaTest = new BenchmarkLazyFastAccessGetInt();
                    lfaTest.Setup();
                    lfaTest.RunWithSpecs(true, BenchmarkLazyFastAccessGetInt.Runs);
                    lfaTest.RunWithSpecs(false, BenchmarkLazyFastAccessGetInt.Runs);
                    break;
                case PerformanceTestTypes.LazyFastAccessOfType:
                    var lfaTypeTest = new BenchmarkLazyFastAccessOfType();
                    lfaTypeTest.Setup();
                    lfaTypeTest.RunWithSpecs(true, BenchmarkLazyFastAccessOfType.Runs);
                    lfaTypeTest.RunWithSpecs(false, BenchmarkLazyFastAccessOfType.Runs);
                    break;
                case PerformanceTestTypes.GenerateJson:
                    var serviceProvider = SetupServiceProvider();
                    serviceProvider.Build<DoFixtureStartup<ScenarioBasic>>();
                    RunTestGenerateJson(serviceProvider);
                    break;
                case PerformanceTestTypes.LoadPresetApp:
                    var x = new BenchmarkPresetApp();
                    x.Setup();
                    x.RunManyTimes(true, false);
                    break;
            }
        }

        Console.WriteLine();
        //tester.LogItems.ToList().ForEach(Console.WriteLine);
        Console.ReadKey();
    }

    private static void RunTestGenerateJson(ServiceProvider serviceProvider)
    {
        var tester = serviceProvider.GetRequiredService<TestGenerateJsonForApps>(); 
        
        // time initial sql
        tester.LoadJsonsFromSql(ScenarioConstants.AppEmptyId);   // first time, to establish connection
        var sqlTime1 = Stopwatch.StartNew();
        var jsonList = tester.LoadJsonsFromSql(ScenarioConstants.AppBig4000Id);   // second time, now with timer
        sqlTime1.Stop();

        var appTime = Stopwatch.StartNew();
        tester.LoadApp(ScenarioConstants.AppBig4000Id);
        appTime.Stop();

        var ignoreWarmUp = tester.SerializeAll(1, jsonList);

        // start timer
        var serTime = Stopwatch.StartNew();
        var count = tester.SerializeAll(250, jsonList);
        serTime.Stop();

        // output
        Console.WriteLine($"app time:{appTime.Elapsed}");
        Console.WriteLine($"sql time:{sqlTime1.Elapsed}");
        Console.WriteLine($"ser time: {serTime.Elapsed} for {count} items");
    }

    public static ServiceProvider SetupServiceProvider()
    {
        var sc = new ServiceCollection();
        new StartupTestsApps().ConfigureServices(sc);
        sc.TryAddTransient<TestGenerateJsonForApps>();
        sc.TryAddTransient<TestLoadPresetApp>();
        sc.AddFixtureHelpers();
        var serviceProvider = sc.BuildServiceProvider();
        return serviceProvider;
    }
}

public enum PerformanceTestTypes
{
    GenerateJson,
    LoadPresetApp,
    LazyFastAccessGetInt,
    LazyFastAccessOfType,
}