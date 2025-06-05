using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.DI;
using ToSic.Testing.Performance.json;

namespace ToSic.Testing.Performance;

class Program
{
    static void Main(string[] args)
    {
        var sc = new ServiceCollection();
        new StartupTestsApps().ConfigureServices(sc);
        sc.TryAddTransient<GenerateJsonForApps>();
        sc.AddFixtureHelpers();
        var serviceProvider = sc.BuildServiceProvider();
        serviceProvider.Build<DoFixtureStartup<ScenarioBasic>>();

        var tester = serviceProvider.GetRequiredService<GenerateJsonForApps>(); 
        
        // time initial sql
        tester.LoadFromSql();   // first time, to establish connection
        var sqlTime1 = Stopwatch.StartNew();
        tester.LoadFromSql();   // second time, now with timer
        sqlTime1.Stop();

        var appTime = Stopwatch.StartNew();
        tester.LoadApp();
        appTime.Stop();

        // start timer
        var serTime = Stopwatch.StartNew();
        var count = tester.SerializeAll(25);
        serTime.Stop();

        // output
        Console.WriteLine($"app time:{appTime.Elapsed}");
        Console.WriteLine($"sql time:{sqlTime1.Elapsed}");
        Console.WriteLine($"ser time: {serTime.Elapsed} for {count} items");
        Console.WriteLine();
        //tester.LogItems.ToList().ForEach(Console.WriteLine);
        Console.ReadKey();
    }
}