using ToSic.Eav.Data.Build.CodeContentTypes;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Build.ContentTypes;

[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypeFactoryAttributesTests(CodeContentTypesManager ctDefFactory)
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
        => ctDefFactory.CreateTac(t).GetDecorator<ContentTypeVirtualAttributes>()!;
        
    [Fact]
    public void Attributes_NoSpec_Count()
        => Equal(4, ctDefFactory.CreateTac<CodeTypeNoSpecs>().Attributes.Count());

    [Fact]
    public void Attributes_NoSpec_CountRecord() =>
        Equal(4, ctDefFactory.CreateTac<CodeTypeNoSpecsRecord>().Attributes.Count());
    
    
    [Fact]
    public void Attributes_NoSpec_NoVDecorator()
        => Null(GetVAttribDecorator(typeof(CodeTypeNoSpecs)));
    [Fact]
    public void Attributes_NoSpec_NoVDecoratorRecord()
        => Null(GetVAttribDecorator(typeof(CodeTypeNoSpecsRecord)));

    
    
    [Theory]
    [InlineData(nameof(CodeTypeNoSpecs.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeNoSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeNoSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeNoSpecs.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => AssertAttribute(ctDefFactory.CreateTac<CodeTypeNoSpecs>(), name, type);

    [Theory]
    [InlineData(nameof(CodeTypeNoSpecs.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeNoSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeNoSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeNoSpecs.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpecRecord(string name, ValueTypes type)
        => AssertAttribute(ctDefFactory.CreateTac<CodeTypeNoSpecsRecord>(), name, type);
    
    
    [Fact]
    public void Attributes_WithSpec_Count()
        => Equal(5, ctDefFactory.CreateTac<CodeTypeWithSpecs>().Attributes.Count());
    
    [Fact]
    public void Attributes_WithSpec_CountRecord()
        => Equal(5, ctDefFactory.CreateTac<CodeTypeWithSpecsRecord>().Attributes.Count());

    
    
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecs.Name) + "Mod", ValueTypes.String, true)]
    [InlineData(nameof(CodeTypeWithSpecs.Url), ValueTypes.Hyperlink)]
    [InlineData(nameof(CodeTypeWithSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeWithSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeWithSpecs.IsAlive), ValueTypes.Boolean, false, CodeTypeWithSpecs.IsAliveDescription)]
    public void AssertAttributeWithSpec(string name, ValueTypes type, bool isTitle = false, string? description = default)
        => AssertAttribute(ctDefFactory.CreateTac<CodeTypeWithSpecs>(), name, type, isTitle, description);
    
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecs.Name) + "Mod", ValueTypes.String, true)]
    [InlineData(nameof(CodeTypeWithSpecs.Url), ValueTypes.Hyperlink)]
    [InlineData(nameof(CodeTypeWithSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeWithSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeWithSpecs.IsAlive), ValueTypes.Boolean, false, CodeTypeWithSpecs.IsAliveDescription)]
    public void AssertAttributeWithSpecRecord(string name, ValueTypes type, bool isTitle = false, string? description = default)
        => AssertAttribute(ctDefFactory.CreateTac<CodeTypeWithSpecsRecord>(), name, type, isTitle, description);

    
    
    /// <summary>
    /// Don't use properties which are private, internal or have the Ignore attribute
    /// </summary>
    /// <param name="name"></param>
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecs.IgnoreThis))]
    [InlineData(nameof(CodeTypeWithSpecs.InternalProperty))]
    [InlineData("PrivateProperty")]
    public void Attributes_WithSpec_SkipIgnores(string name)
        => DoesNotContain(name, ctDefFactory.CreateTac<CodeTypeWithSpecs>().Attributes.Select(a => a.Name));
    
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecsRecord.IgnoreThis))]
    [InlineData(nameof(CodeTypeWithSpecsRecord.InternalProperty))]
    [InlineData("PrivateProperty")]
    public void Attributes_WithSpec_SkipIgnoresRecord(string name)
        => DoesNotContain(name, ctDefFactory.CreateTac<CodeTypeWithSpecsRecord>().Attributes.Select(a => a.Name));

    
    
    [Fact]
    public void Attributes_WithSpec_VDecoratorHas() =>
        NotNull(GetVAttribDecorator(typeof(CodeTypeWithSpecs)));
    [Fact]
    public void Attributes_WithSpec_VDecoratorHasRecord() =>
        NotNull(GetVAttribDecorator(typeof(CodeTypeWithSpecsRecord)));

    
    
    [Fact]
    public void Attributes_WithSpec_VDecoratorExactly2() =>
        Equal(2, GetVAttribDecorator(typeof(CodeTypeWithSpecs))?.VirtualAttributes.Count);

    [Fact]
    public void Attributes_WithSpec_VDecoratorExactly2Record() =>
        Equal(2, GetVAttribDecorator(typeof(CodeTypeWithSpecsRecord))?.VirtualAttributes.Count);

    
    
    [Fact]
    public void Attributes_InternalFields()
    {
        var x = ctDefFactory.CreateTac<CodeTypeInternalFields>();
        Single(x.Attributes);
    }
}
