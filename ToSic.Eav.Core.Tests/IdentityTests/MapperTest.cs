using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ToSic.Eav.Identity;

namespace ToSic.Eav.Core.Tests.IdentityTests
{
    [TestClass]
    public class MapperTest
    {
        private string Compress(Guid value) => Mapper.GuidCompress(value);
        private Guid UnCompress(string value) => Mapper.GuidRestore(value);

        [TestMethod]
        public void CompressAndUnCompress()
        {
            var data = Guid.NewGuid();
            var compressed = Compress(data);
            var uncompressed = UnCompress(compressed);
            Assert.AreEqual(data, uncompressed);
        }
    }
}
