using System.Diagnostics;
using ToSic.Eav.StartupTests;

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupTestsEavCoreAndDataSources))]
public class PassThrough_Cache(DataSourcesTstBuilder dsSvc)
{
    [Fact]
    public void PassThrough_CacheKey()
    {
        var outSource = dsSvc.CreateDataSource<PassThrough>();
        var partialKey = outSource.CachePartialKey;
        var fullKey = outSource.CacheFullKey;
        Trace.WriteLine("Partial Key:" + partialKey);
        Trace.WriteLine("Full Key: " + fullKey);
        NotNull(partialKey);
        NotNull(fullKey);
    }

}