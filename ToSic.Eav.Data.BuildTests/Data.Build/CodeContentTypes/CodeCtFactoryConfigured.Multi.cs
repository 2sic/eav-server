using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

[Startup(typeof(StartupTestsEavDataBuild))]
// ReSharper disable once UnusedMember.Global
public class CodeCtFactoryConfiguredClass(CodeContentTypesManager ctDefMan)
    : CodeCtFactoryConfigured<CodeTypeWithSpecs>(ctDefMan);

[Startup(typeof(StartupTestsEavDataBuild))]
// ReSharper disable once UnusedMember.Global
public class CodeCtFactoryConfiguredRecord(CodeContentTypesManager ctDefMan)
    : CodeCtFactoryConfigured<CodeTypeWithSpecsRecord>(ctDefMan);


/// <summary>
/// Tests for configured classes (with attributes)
/// </summary>
/// <param name="ctDefManager"></param>
public abstract class CodeCtFactoryConfigured<TRawEntity>(CodeContentTypesManager ctDefManager)
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
        => ctDefManager.CreateTac(t).GetDecorator<ContentTypeVirtualAttributes>()!;
    
    
    
    [Fact]
    public void Attributes_WithSpec_Count()
        => Equal(5, ctDefManager.CreateTac<TRawEntity>().Attributes.Count());
    
    
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecs.Name) + "Mod", ValueTypes.String, true)]
    [InlineData(nameof(CodeTypeWithSpecs.Url), ValueTypes.Hyperlink)]
    [InlineData(nameof(CodeTypeWithSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeWithSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeWithSpecs.IsAlive), ValueTypes.Boolean, false, CodeTypeWithSpecs.IsAliveDescription)]
    public void AssertAttributeWithSpec(string name, ValueTypes type, bool isTitle = false, string? description = default)
        => AssertAttribute(ctDefManager.CreateTac<TRawEntity>(), name, type, isTitle, description);
    
    /// <summary>
    /// Don't use properties which are private, internal or have the Ignore attribute
    /// </summary>
    /// <param name="name"></param>
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecs.IgnoreThis))]
    [InlineData(nameof(CodeTypeWithSpecs.InternalProperty))]
    [InlineData("PrivateProperty")]
    public void Attributes_WithSpec_SkipIgnores(string name)
        => DoesNotContain(name, ctDefManager.CreateTac<TRawEntity>().Attributes.Select(a => a.Name));
    
   
    
    [Fact]
    public void Attributes_WithSpec_VDecoratorHas() =>
        NotNull(GetVAttribDecorator(typeof(TRawEntity)));
    
    
    [Fact]
    public void Attributes_WithSpec_VDecoratorExactly2() =>
        Equal(2, GetVAttribDecorator(typeof(TRawEntity))?.VirtualAttributes.Count);

}
