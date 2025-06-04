using ToSic.Eav.Data.EntityDecorators.Sys;

namespace ToSic.Eav.Serialization;

public class EntitySerializationDecoratorCreatorTests
{
    private EntitySerializationDecorator FromFieldListTac(List<string> selectFields, bool withGuid = false)
        => new EntitySerializationDecoratorCreator(selectFields, withGuid, null).Generate();

    private void AllPropsAreNull(EntitySerializationDecorator decorator, string skip = default)
    {
        if (skip != "id")
            Null(decorator.SerializeId);
        if (skip != "guid")
            Null(decorator.SerializeGuid);
        if (skip != "title")
            Null(decorator.SerializeTitle);
        if (skip != "modified")
            Null(decorator.SerializeModified);
        if (skip != "created")
            Null(decorator.SerializeCreated);
        if (skip != "appid")
            Null(decorator.SerializeAppId);
        if (skip != "fields")
        {
            Null(decorator.FilterFields);
            Null(decorator.FilterFieldsEnabled);
        }
    }
    private EntitySerializationDecorator AllPropsNullReuse(EntitySerializationDecorator decorator, string skip = default)
    {
        AllPropsAreNull(decorator, skip);
        return decorator;
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData(new string[0], "empty array")]
    [InlineData(new string[] {null!}, "null in array")]
    [InlineData(new[] {""}, "string empty in array")]
    [InlineData(new[] {" "}, "space")]
    public void EmptyFieldListGuidFalseIsGuidNull(string[] values, string testName)
        => AllPropsAreNull(FromFieldListTac(values! == null ? null : [..values], false));


    [Fact]
    public void NoFieldsGuidTrueIsGuidTrue()
        => True(AllPropsNullReuse(FromFieldListTac([], true), "guid").SerializeGuid);

    [Fact]
    public void NoFieldsNullGuidTrueIsGuidTrue()
        => True(AllPropsNullReuse(FromFieldListTac(null!, true), "guid").SerializeGuid);


    [Fact]
    public void FieldIdSerializeIdTrue()
        => True(FromFieldListTac(["id"]).SerializeId);

    [Fact]
    public void FieldIdSerializeGuidFalse()
        => False(FromFieldListTac(["id"]).SerializeGuid);

    [Theory]
    [InlineData("guid")]
    [InlineData("Guid")]
    [InlineData("GUID")]
    [InlineData("+guid")]
    public void FieldGuidSerializeGuidTrue(string oneName)
        => True(FromFieldListTac([oneName]).SerializeGuid);

    [Theory]
    [InlineData(new []{"Id", "Guid", "FirstName", "LastNAME"}, new []{"firstname", "lastname"})]
    [InlineData(new []{"Id", "Guid", "FirstName", "Modified"}, new []{"firstname"})]
    public void FieldListIsExpected(string[] fields, string[] expected)
        => Equal(expected, FromFieldListTac([..fields]).FilterFields);

}

