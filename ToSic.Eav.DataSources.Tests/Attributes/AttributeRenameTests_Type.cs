using ToSic.Eav.TestData;

namespace ToSic.Eav.DataSourceTests;

public partial class AttributeRenameTests
{

    [TestMethod]
    public void TypeChange()
    {
        var attRen = new AttributeRenameTester(this).CreateRenamer(10);
        attRen.TypeName = "MyNiceTypeName";
        var result = attRen.ListTac().ToList();
        AreEqual(10, result.Count);
        var item = result.First();
        AreEqual("MyNiceTypeName", item.Type.Name, "Typename should have changed");

        AreEqual(PersonSpecs.ValueColumns, item.Attributes.Count);
        IsTrue(item.Attributes.ContainsKey(PersonSpecs.FieldFullName));
        IsFalse(item.Attributes.ContainsKey(ShortName));

    }



}