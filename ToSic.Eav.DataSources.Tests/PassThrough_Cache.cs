using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.DataSources.Tests
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
            var outSource = new PassThrough();
            var partialKey = outSource.CachePartialKey;
            var fullKey = outSource.CacheFullKey;
            Assert.IsNotNull(partialKey);
        }



        #endregion
        
    }
}
