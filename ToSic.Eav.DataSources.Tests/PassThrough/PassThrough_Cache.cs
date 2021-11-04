using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class PassThrough_Cache: TestBaseDiEavFull
    {

        #region bool tests


        [TestMethod]
        public void PassThrough_CacheKey()
        {
            var outSource = Build<DataSources.PassThrough>();
            outSource.Init(new LookUpEngine(null));
            var partialKey = outSource.CachePartialKey;
            var fullKey = outSource.CacheFullKey;
            Trace.WriteLine("Partial Key:" + partialKey);
            Trace.WriteLine("Full Key: " + fullKey);
            Assert.IsNotNull(partialKey);
            Assert.IsNotNull(fullKey);
        }



        #endregion
        
    }
}
