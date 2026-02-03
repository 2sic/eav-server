using System.Xml.Linq;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.CodeContentTypes;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Builder;

[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypeFactoryAttributesTests(ContentTypeFactory factory)
{
    private void AssertAttribute(IContentType ct, string name, ValueTypes type, bool isTitle = false, string? description = default)
    {
        var attribute = ct.Attributes.FirstOrDefault(a => a.Name == name);
        NotNull(attribute); //, $"{name} null check");
        Equal(name, attribute.Name); //, $"{name} Name check");
        Equal(type, attribute.Type); //, $"{name} type check");
        Equal(isTitle, attribute.IsTitle); //, $"{name} IsTitle check");
        if (description != default)
            Equal(description, attribute.Metadata.Get<string>(AttributeMetadataConstants.DescriptionField)); //, $"{name} Description check");
    }

    private ContentTypeVirtualAttributes GetVAttribDecorator(Type t)
        => factory.CreateTac(t).GetDecorator<ContentTypeVirtualAttributes>()!;
        
    [Fact]
    public void Attributes_NoSpec_Count()
        => Equal(4, factory.CreateTac<CodeTypeNoSpecs>().Attributes.Count());

    [Fact]
    public void Attributes_NoSpec_NoVDecorator()
        => Null(GetVAttribDecorator(typeof(CodeTypeNoSpecs)));

    [Theory]
    [InlineData(nameof(CodeTypeNoSpecs.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeNoSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeNoSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeNoSpecs.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => AssertAttribute(factory.CreateTac<CodeTypeNoSpecs>(), name, type);

    [Fact]
    public void Attributes_WithSpec_Count()
        => Equal(5, factory.CreateTac<CodeTypeWithSpecs>().Attributes.Count());

    [Theory]
    [InlineData(nameof(CodeTypeWithSpecs.Name) + "Mod", ValueTypes.String, true)]
    [InlineData(nameof(CodeTypeWithSpecs.Url), ValueTypes.Hyperlink)]
    [InlineData(nameof(CodeTypeWithSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeWithSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeWithSpecs.IsAlive), ValueTypes.Boolean, false, CodeTypeWithSpecs.IsAliveDescription)]
    public void AssertAttributeWithSpec(string name, ValueTypes type, bool isTitle = false, string? description = default)
        => AssertAttribute(factory.CreateTac<CodeTypeWithSpecs>(), name, type, isTitle, description);

    /// <summary>
    /// Don't use properties which are private, internal or have the Ignore attribute
    /// </summary>
    /// <param name="name"></param>
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecs.IgnoreThis))]
    [InlineData(nameof(CodeTypeWithSpecs.InternalProperty))]
    [InlineData("PrivateProperty")]
    public void Attributes_WithSpec_SkipIgnores(string name)
        => DoesNotContain(name, factory.CreateTac<CodeTypeWithSpecs>().Attributes.Select(a => a.Name));
        //=> False(factory.CreateTac<CodeTypeWithSpecs>().Attributes.Any(a => a.Name == name));

    [Fact]
    public void Attributes_WithSpec_VDecoratorHas() => NotNull(GetVAttribDecorator(typeof(CodeTypeWithSpecs)));

    [Fact]
    public void Attributes_WithSpec_VDecoratorExactly2() => Equal(2, GetVAttribDecorator(typeof(CodeTypeWithSpecs))?.VirtualAttributes.Count);

    [Fact]
    public void Attributes_InternalFields()
    {
        var x = factory.CreateTac<CodeTypeInternalFields>();
        Single(x.Attributes);
    }
}
