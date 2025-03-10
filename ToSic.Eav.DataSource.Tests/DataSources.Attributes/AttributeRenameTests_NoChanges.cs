using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSources.Attributes;

public partial class AttributeRenameTests
{
    [Fact]
    public void DefaultConfiguration()
    {
        var attrRename = dsSvc.CreateDataSource<AttributeRename>();
        attrRename.Configuration.Parse();
        Equal(true, attrRename.KeepOtherAttributes);
        Equal("", attrRename.AttributeMap);
        Equal("", attrRename.TypeName);
    }

    [Fact]
    public void DefaultWithoutMap()
    {
        var attRenCompare = attributeRenameTester.CreateRenamer(10);
        var item = attRenCompare.ListTac().First();
        AssertHasFields(item, PersonSpecs.Fields);
        Equal(PersonSpecs.PersonTypeName, item.Type.Name);//, "Typename should not change");
    }

    [Fact]
    public void NoChanges()
    {
        var attRen = attributeRenameTester.CreateRenamer(10);
        var result = attRen.ListTac().ToList();
        Equal(10, result.Count);
        var item = result.First();
        Equal(PersonSpecs.ValueColumns, item.Attributes.Count);
        True(item.Attributes.ContainsKey(PersonSpecs.FieldFullName));
        False(item.Attributes.ContainsKey(ShortName));
    }


}