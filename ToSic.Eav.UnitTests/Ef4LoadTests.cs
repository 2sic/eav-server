using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Repository.EF4;

namespace ToSic.Eav.Persistence.EFC11.Tests
{
    [TestClass]
    public class Ef4LoadTests
    {

        #region test preparations

        private DbDataController _db;
        private Ef4Loader _loader;

        [TestInitialize]
        public void Init()
        {
            _db = DbDataController.Instance(null, 2);
            _loader = new Ef4Loader(_db);
        }
        #endregion

        [TestMethod]
        public void TestLoadXApp2()
        {
            var result = _loader.CompleteApp(2, null, null, false);
            Assert.AreEqual(1063, result.Entities.Count, "counting...");
        }

        [TestMethod]
        public void TryToLoadCtsOf2()
        {
            _loader.ResetCacheForTesting();
            var results = _loader.ContentTypes(2);
            Assert.AreEqual(61, results.Count, "dummy test: ");
        }

        [TestMethod]
        public void TryToLoadCtsOf2Again()
        {
            _loader.ResetCacheForTesting();
            var results = _loader.ContentTypes(2);
            Assert.AreEqual(61, results.Count, "dummy test: ");
        }
        [TestMethod]
        public void TryToLoadCtsOf2TenXCleared()
        {
            var results = _loader.ContentTypes(2);
            for (var x = 0; x < 9; x++)
            {
                _loader.ResetCacheForTesting();
                results = _loader.ContentTypes(2);
            }
            Assert.AreEqual(61, results.Count, "dummy test: ");
        }
    }
}
