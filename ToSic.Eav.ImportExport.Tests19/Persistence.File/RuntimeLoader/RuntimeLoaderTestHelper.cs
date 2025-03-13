using System.Diagnostics;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;

class RuntimeLoaderTestHelper
{
    public static void TestWithXContentTypes(IAppReader globalAppState, ITestOutputHelper output, int min, int max)
    {
        // set loader root path, based on test environment
        var time = Stopwatch.StartNew();
        var count = globalAppState.ContentTypes.Count();
        time.Stop();

        InRange(count, min, max);
        output.WriteLine($"time used: {time.Elapsed} expected between {min} and {max}, actually is {count}");
    }

    public static void TestConcurrentInitializeGlobalAppState(IAppReader globalAppState, ITestOutputHelper output, int expected)
    {
        int res1 = 0, res2 = 0;

        var t1 = new Thread(() => res1 = globalAppState.ContentTypes.Count());
        var t2 = new Thread(() => res2 = globalAppState.ContentTypes.Count());

        t1.Start();
        t2.Start();
        t1.Join();
        t2.Join();
        Equal(expected, res1); // > 0, "should have gotten a count");
        Equal(res1, res2);//, "both res should be equal");
        output.WriteLine($"res1: {res1}, res2: {res2}");
    }

    /// <summary>
    /// Note: this is not really done TODO: TEST DOESN'T QUITE WORK
    /// </summary>
    public static void TestWith3FileTypes(IAppReader globalAppState, ITestOutputHelper output, int expected)
    {
        // set loader root path, based on test environment
        //AdditionalGlobalFolderRepositoryForReflection.PathToUse = TestFiles.GetTestPath(PersistenceTestConstants.ScenarioMini); // PersistenceTestConstants.TestStorageRoot(TestContext);

        var all = globalAppState.ContentTypes.ToList();
        Equal(expected, all.Count);

        var hasCodeSql = all.FirstOrDefault(t => t.Name.Contains("SqlData"));
        NotNull(hasCodeSql);//, "should find code sql");

        True(hasCodeSql is TypesBase, "sql should come from code, and not from json, as code has higher priority");

        var whateverType = all.FirstOrDefault(t => t.Name == "Whatever");
        NotNull(whateverType);//, "should find whatever type from json");
        //var dummy = all.First();
        //Assert.Equal(DemoType.CTypeName, dummy.Key);
    }

}