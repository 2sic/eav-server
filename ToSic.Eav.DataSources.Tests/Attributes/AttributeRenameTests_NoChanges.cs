using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.ExternalData;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSourceTests.Attributes
{
    public partial class AttributeRenameTests
    {
        [TestMethod]
        public void DefaultConfiguration()
        {
            var attrRename = Factory.Resolve<DataSourceFactory>().GetDataSource<AttributeRename>(new AppIdentity(1, 1), null, new LookUpEngine(null as ILog));
            attrRename.Configuration.Parse();
            Assert.AreEqual(true, attrRename.KeepOtherAttributes);
            Assert.AreEqual("", attrRename.AttributeMap);
            Assert.AreEqual("", attrRename.TypeName);
        }

        [TestMethod]
        public void DefaultWithoutMap()
        {
            var attRenCompare = AttributeRenameTester.CreateRenamer(10);
            var item = attRenCompare.Immutable.First();
            AssertHasFields(item, DataTableTst.Fields);
            Assert.AreEqual(DataTableTst.ContentTypeName, item.Type.Name, "Typename should not change");
        }

        [TestMethod]
        public void NoChanges()
        {
            var attRen = AttributeRenameTester.CreateRenamer(10);
            var result = attRen.Immutable.ToList();
            Assert.AreEqual(10, result.Count);
            var item = result.First();
            Assert.AreEqual(DataTableTst.ValueColumns, item.Attributes.Count);
            Assert.IsTrue(item.Attributes.ContainsKey(DataTableTst.FieldFullName));
            Assert.IsFalse(item.Attributes.ContainsKey(ShortName));
        }


    }
}
