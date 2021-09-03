using System;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Tests;
using ToSic.Testing.Performance.json;
using ToSic.Testing.Shared;

namespace ToSic.Testing.Performance
{
    class Program // : Efc11TestBase
    {
        //public Program(IServiceProvider serviceProvider) : base(serviceProvider)
        //{
        //}

        static void Main(string[] args)
        {
            // initialize everything
            Shared.StartupTestingShared.ConfigureEfcDi();

            var tester = EavTestBase.Resolve<GenerateJsonForApps>();
            tester.Init();

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
            tester.LogItems.ToList().ForEach(Console.WriteLine);
            Console.ReadKey();
        }
    }
}
