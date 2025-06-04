using ToSic.Eav.Data.Attributes.Sys;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Builder.ClassesWithTypeDecoration;
using ToSic.Eav.Data.ContentTypes.Sys;

namespace ToSic.Eav.Data.Builder;

[Startup(typeof(StartupTestsEavCore))]
public class ContentTypeFactoryAttributesTests(ContentTypeFactory factory)
{
    private void AssertAttribute(IContentType ct, string name, ValueTypes type, bool isTitle = false, string description = default)
    {
        var attribute = ct.Attributes.FirstOrDefault(a => a.Name == name);
        NotNull(attribute); //, $"{name} null check");
        Equal(name, attribute.Name); //, $"{name} Name check");
        Equal(type, attribute.Type); //, $"{name} type check");
        Equal(isTitle, attribute.IsTitle); //, $"{name} IsTitle check");
        if (description != default)
            Equal(description, attribute.Metadata.GetBestValue<string>(AttributeMetadataConstants.DescriptionField)); //, $"{name} Description check");
    }

    private ContentTypeVirtualAttributes GetVAttribDecorator(Type t) => factory.Create(t).GetDecorator<ContentTypeVirtualAttributes>();
        
    [Fact]
    public void Attributes_NoSpec_Count() => Equal(4, factory.Create(typeof(TestTypeNoSpecs)).Attributes.Count());

    [Fact]
    public void Attributes_NoSpec_NoVDecorator() => Null(GetVAttribDecorator(typeof(TestTypeNoSpecs)));

    [Theory]
    [InlineData(nameof(TestTypeNoSpecs.Name), ValueTypes.String)]
    [InlineData(nameof(TestTypeNoSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(TestTypeNoSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(TestTypeNoSpecs.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => AssertAttribute(factory.Create(typeof(TestTypeNoSpecs)), name, type);

    [Fact]
    public void Attributes_WithSpec_Count() => Equal(5, factory.Create(typeof(TestTypeWithSpecs)).Attributes.Count());

    [Theory]
    [InlineData(nameof(TestTypeWithSpecs.Name) + "Mod", ValueTypes.String, true)]
    [InlineData(nameof(TestTypeWithSpecs.Url), ValueTypes.Hyperlink)]
    [InlineData(nameof(TestTypeWithSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(TestTypeWithSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(TestTypeWithSpecs.IsAlive), ValueTypes.Boolean, false, TestTypeWithSpecs.IsAliveDescription)]
    public void AssertAttributeWithSpec(string name, ValueTypes type, bool isTitle = false, string description = default)
        => AssertAttribute(factory.Create(typeof(TestTypeWithSpecs)), name, type, isTitle, description);

    /// <summary>
    /// Don't use properties which are private, internal or have the Ignore attribute
    /// </summary>
    /// <param name="name"></param>
    [Theory]
    [InlineData(nameof(TestTypeWithSpecs.IgnoreThis))]
    [InlineData(nameof(TestTypeWithSpecs.InternalProperty))]
    [InlineData("PrivateProperty")]
    public void Attributes_WithSpec_SkipIgnores(string name)
        => False(factory.Create(typeof(TestTypeWithSpecs)).Attributes.Any(a => a.Name == name));

    [Fact]
    public void Attributes_WithSpec_VDecoratorHas() => NotNull(GetVAttribDecorator(typeof(TestTypeWithSpecs)));

    [Fact]
    public void Attributes_WithSpec_VDecoratorExactly2() => Equal(2, GetVAttribDecorator(typeof(TestTypeWithSpecs))?.VirtualAttributes.Count);

    [Fact]
    public void Attributes_InternalFields()
    {
        var x = factory.Create(typeof(TestTypeInternalFields));
        Single(x.Attributes);
    }
}
