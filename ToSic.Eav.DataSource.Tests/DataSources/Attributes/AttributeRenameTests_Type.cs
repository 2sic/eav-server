using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSources.Attributes;

public partial class AttributeRenameTests
{

    [Fact]
    public void TypeChange()
    {
        var attRen = attributeRenameTester.CreateRenamer(10);
        attRen.TypeName = "MyNiceTypeName";
        var result = attRen.ListTac().ToList();
        Equal(10, result.Count);
        var item = result.First();
        Equal("MyNiceTypeName", item.Type.Name);//, "Typename should have changed");

        Equal(PersonSpecs.ValueColumns, item.Attributes.Count);
        True(item.Attributes.ContainsKey(PersonSpecs.FieldFullName));
        False(item.Attributes.ContainsKey(ShortName));

    }



}