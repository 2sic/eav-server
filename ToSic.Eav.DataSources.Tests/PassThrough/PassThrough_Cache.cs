using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSourceTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class PassThrough_Cache
    {

        #region bool tests


        [TestMethod]
        public void PassThrough_CacheKey()
        {
            var outSource = new DataSources.PassThrough();
            outSource.Configuration.LookUpEngine = new LookUpEngine(null);
            var partialKey = outSource.CachePartialKey;
            var fullKey = outSource.CacheFullKey;
            Assert.IsNotNull(partialKey);
            Assert.IsNotNull(fullKey);
        }



        #endregion
        
    }
}
