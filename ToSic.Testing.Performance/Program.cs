using System;
using System.Diagnostics;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.DI;
using ToSic.Testing.Performance.json;
using ToSic.Testing.Performance.LoadPresetApp;

namespace ToSic.Testing.Performance;

class Program
{
    public const bool RunGenerateJson = false;
    public const int RunLoadPresets = 25;

    public static bool ModeBenchmark = false;

    static void Main(string[] args)
    {
        var serviceProvider = SetupServiceProvider();
        serviceProvider.Build<DoFixtureStartup<ScenarioBasic>>();

        // When running in benchmark mode, we run the benchmarks and nothing else
        if (ModeBenchmark)
        {
            var summary = BenchmarkRunner.Run<BenchmarkPresetApp>();
        }
        // Otherwise - usually when running with the performance profiler - we run the tests
        else
        {
            if (RunGenerateJson)
                RunTestGenerateJson(serviceProvider);

            if (RunLoadPresets > 0)
            {
                var x = new BenchmarkPresetApp();
                x.Setup();
                x.RunManyTimes(true, false);
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