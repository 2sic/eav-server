using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.Attributes
{
    public partial class AttributeRenameTests
    {

        [TestMethod]
        public void TypeChange()
        {
            var attRen = AttributeRenameTester.CreateRenamer(10);
            attRen.TypeName = "MyNiceTypeName";
            var result = attRen.Immutable.ToList();
            Assert.AreEqual(10, result.Count);
            var item = result.First();
            Assert.AreEqual("MyNiceTypeName", item.Type.Name, "Typename should have changed");

            Assert.AreEqual(DataTableTst.ValueColumns, item.Attributes.Count);
            Assert.IsTrue(item.Attributes.ContainsKey(DataTableTst.FieldFullName));
            Assert.IsFalse(item.Attributes.ContainsKey(ShortName));

        }



    }
}
