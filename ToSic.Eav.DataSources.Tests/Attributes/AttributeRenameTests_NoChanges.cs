namespace ToSic.Eav.DataSourceTests;

public partial class AttributeRenameTests
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void DefaultConfiguration()
    {
        var attrRename = DsSvc.CreateDataSource<AttributeRename>();
        attrRename.Configuration.Parse();
        AreEqual(true, attrRename.KeepOtherAttributes);
        AreEqual("", attrRename.AttributeMap);
        AreEqual("", attrRename.TypeName);
    }

    [TestMethod]
    public void DefaultWithoutMap()
    {
        var attRenCompare = new AttributeRenameTester(this).CreateRenamer(10);
        var item = attRenCompare.ListTac().First();
        AssertHasFields(item, PersonSpecs.Fields);
        AreEqual(PersonSpecs.PersonTypeName, item.Type.Name, "Typename should not change");
    }

    [TestMethod]
    public void NoChanges()
    {
        var attRen = new AttributeRenameTester(this).CreateRenamer(10);
        var result = attRen.ListTac().ToList();
        AreEqual(10, result.Count);
        var item = result.First();
        AreEqual(PersonSpecs.ValueColumns, item.Attributes.Count);
        IsTrue(item.Attributes.ContainsKey(PersonSpecs.FieldFullName));
        IsFalse(item.Attributes.ContainsKey(ShortName));
    }


}