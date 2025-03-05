using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSourceTests.TestData;

namespace ToSic.Eav.DataSourceTests;

public partial class AttributeRenameTests
{

    [TestMethod]
    public void TypeChange()
    {
        var attRen = new AttributeRenameTester(this).CreateRenamer(10);
        attRen.TypeName = "MyNiceTypeName";
        var result = attRen.ListTac().ToList();
        Assert.AreEqual(10, result.Count);
        var item = result.First();
        Assert.AreEqual("MyNiceTypeName", item.Type.Name, "Typename should have changed");

        Assert.AreEqual(PersonSpecs.ValueColumns, item.Attributes.Count);
        Assert.IsTrue(item.Attributes.ContainsKey(PersonSpecs.FieldFullName));
        Assert.IsFalse(item.Attributes.ContainsKey(ShortName));

    }



}