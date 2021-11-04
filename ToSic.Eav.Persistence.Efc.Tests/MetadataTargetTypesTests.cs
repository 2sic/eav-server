using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class MetadataTargetTypesTests : Efc11TestBase
    {
        private readonly ITargetTypes _targetTypes;
        public MetadataTargetTypesTests()
        {
            _targetTypes = Build<ITargetTypes>();
        }
        
        [TestMethod]
        public void TestMetadataTargetTypes()
        {
            var types = _targetTypes.TargetTypes;

            Assert.AreEqual(100, types.Count);
            Assert.IsTrue(types[(int)TargetTypes.None] == "Default");
        }
        
        
    }
}
