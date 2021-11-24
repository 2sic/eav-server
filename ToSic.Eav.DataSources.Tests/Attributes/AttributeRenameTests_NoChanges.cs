using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSourceTests
{
    public partial class AttributeRenameTests
    {
        [TestMethod]
        public void DefaultConfiguration()
        {
            var attrRename = DataSourceFactory.GetDataSource<AttributeRename>(new AppIdentity(1, 1), null, new LookUpEngine(null as ILog));
            attrRename.Configuration.Parse();
            Assert.AreEqual(true, attrRename.KeepOtherAttributes);
            Assert.AreEqual("", attrRename.AttributeMap);
            Assert.AreEqual("", attrRename.TypeName);
        }

        [TestMethod]
        public void DefaultWithoutMap()
        {
            var attRenCompare = new AttributeRenameTester(this).CreateRenamer(10);
            var item = attRenCompare.ListForTests().First();
            AssertHasFields(item, PersonSpecs.Fields);
            Assert.AreEqual(PersonSpecs.PersonTypeName, item.Type.Name, "Typename should not change");
        }

        [TestMethod]
        public void NoChanges()
        {
            var attRen = new AttributeRenameTester(this).CreateRenamer(10);
            var result = attRen.ListForTests().ToList();
            Assert.AreEqual(10, result.Count);
            var item = result.First();
            Assert.AreEqual(PersonSpecs.ValueColumns, item.Attributes.Count);
            Assert.IsTrue(item.Attributes.ContainsKey(PersonSpecs.FieldFullName));
            Assert.IsFalse(item.Attributes.ContainsKey(ShortName));
        }


    }
}
