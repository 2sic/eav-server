using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.Data.Build.CodeContentTypes;
// ReSharper disable UnusedMember.Global

[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfiguredClass(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigured<CodeTypeWithSpecs>(ctDefManager);

[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfiguredRecord(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigured<CodeTypeWithSpecsRecord>(ctDefManager);

// ReSharper restore UnusedMember.Global

/// <summary>
/// Shared Tests for configured classes/records (with attributes).
/// Both test samples (class and record) must have the same attributes, so we can use the same tests for both of them.
/// </summary>
/// <param name="ctDefManager"></param>
public abstract class CodeCtFactoryConfigured<TRawEntity>(CodeContentTypesManager ctDefManager)
{
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
        => ctDefManager.CreateTac<TRawEntity>().AssertAttribute(name, type, isTitle, description);
    
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
        NotNull(ctDefManager.GetVirtualAttribDecorator(typeof(TRawEntity)));
    
    
    [Fact]
    public void Attributes_WithSpec_VDecoratorExactly2() =>
        Equal(2, ctDefManager.GetVirtualAttribDecorator(typeof(TRawEntity)).VirtualAttributes.Count);

}
