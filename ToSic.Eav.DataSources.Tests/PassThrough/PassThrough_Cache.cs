using System.Diagnostics;

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[TestClass]
public class PassThrough_Cache: TestBaseEavDataSource
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();


    [TestMethod]
    public void PassThrough_CacheKey()
    {
        var outSource = DsSvc.CreateDataSource<PassThrough>();
        var partialKey = outSource.CachePartialKey;
        var fullKey = outSource.CacheFullKey;
        Trace.WriteLine("Partial Key:" + partialKey);
        Trace.WriteLine("Full Key: " + fullKey);
        IsNotNull(partialKey);
        IsNotNull(fullKey);
    }

}