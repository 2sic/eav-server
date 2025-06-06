using System;
using System.Diagnostics;
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
    public const int RunLoadPresets = 10;

    static void Main(string[] args)
    {
        var serviceProvider = SetupServiceProvider();
        serviceProvider.Build<DoFixtureStartup<ScenarioBasic>>();

        if (RunGenerateJson)
            RunTestGenerateJson(serviceProvider);

        if (RunLoadPresets > 0)
        {
            var tester = serviceProvider.GetRequiredService<TestLoadPresetApp>();
            var timer = Stopwatch.StartNew();
            for (var i = 0; i < RunLoadPresets; i++)
            {
                var runTimer = Stopwatch.StartNew();
                tester.Run();
                runTimer.Stop();
                Console.WriteLine($@"Run {i + 1} done. Time taken: {runTimer.Elapsed}");
            }
            timer.Stop();
            // calculate average time
            var averageTime = TimeSpan.FromTicks(timer.Elapsed.Ticks / RunLoadPresets);
            Console.WriteLine($@"Total time for {RunLoadPresets} runs: {timer.Elapsed}; Average: {averageTime}");
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

    private static ServiceProvider SetupServiceProvider()
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