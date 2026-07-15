using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Build.CodeContentTypes;
// ReSharper disable UnusedMember.Global

[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfiguredClass(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigured<CodeTypeWithSpecsClass>(ctDefManager);

[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfiguredRecord(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigured<CodeTypeWithSpecsRecord>(ctDefManager);

// ReSharper restore UnusedMember.Global

/// <summary>
/// Shared Tests for configured classes/records (with attributes).
/// Both test samples (class and record) must have the same attributes, so we can use the same tests for both of them.
/// </summary>
/// <param name="ctDefManager"></param>
public abstract class CodeCtFactoryConfigured<TCodeTypeWithSpecs>(CodeContentTypesManager ctDefManager)
{
    [Fact]
    public void Attributes_WithSpec_Count()
        => Equal(5, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Attributes.Count());
    
    
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecsClass.Name) + "Mod", ValueTypes.String, true)]
    [InlineData(nameof(CodeTypeWithSpecsClass.Url), ValueTypes.Hyperlink)]
    [InlineData(nameof(CodeTypeWithSpecsClass.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeWithSpecsClass.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeWithSpecsClass.IsAlive), ValueTypes.Boolean, false, CodeTypeWithSpecsClass.IsAliveDescription)]
    public void AssertAttributeWithSpec(string name, ValueTypes type, bool isTitle = false, string? description = default)
        => ctDefManager.CreateTac<TCodeTypeWithSpecs>().AssertAttributeDefinition(name, type, isTitle, description);
    
    /// <summary>
    /// Don't use properties which are private, internal or have the Ignore attribute
    /// </summary>
    /// <param name="name"></param>
    [Theory]
    [InlineData(nameof(CodeTypeWithSpecsClass.IgnoreThis))]
    [InlineData(nameof(CodeTypeWithSpecsClass.InternalProperty))]
    [InlineData("PrivateProperty")]
    public void Attributes_WithSpec_SkipIgnores(string name)
        => DoesNotContain(name, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Attributes.Select(a => a.Name));
    
   
    
    [Fact]
    public void Attributes_WithSpec_VDecoratorHas() =>
        NotNull(ctDefManager.GetVirtualAttribDecorator(typeof(TCodeTypeWithSpecs)));
    
    
    [Fact]
    public void Attributes_WithSpec_VDecoratorExactly2() =>
        Equal(2, ctDefManager.GetVirtualAttribDecorator(typeof(TCodeTypeWithSpecs)).VirtualAttributes.Count);

    #region Inspect ContentType of generated data

    [Fact]
    public void ContentType_IsNotNull()
        => NotNull(ctDefManager.CreateTac<TCodeTypeWithSpecs>());

    [Fact]
    public void ContentType_IsNotDynamic()
        => False(ctDefManager.CreateTac<TCodeTypeWithSpecs>().IsDynamic);

    [Fact]
    public void ContentType_HasRepositoryTypeCodeConfiguration()
        => Equal(RepositoryTypes.CodeConfiguration, ctDefManager.CreateTac<TCodeTypeWithSpecs>().RepositoryType);

    [Fact]
    public void ContentType_DoesNotAlwaysShareConfiguration()
        => False(ctDefManager.CreateTac<TCodeTypeWithSpecs>().AlwaysShareConfiguration);

    [Fact]
    public void ContentType_HasNullSysSettings()
        => Null(ctDefManager.CreateTac<TCodeTypeWithSpecs>().SysSettings);

    #endregion

    #region ContentType Specs match what is expected

    [Fact]
    public void ContentType_NameIsPreconfigured()
        => Equal(CodeTypeWithSpecsEmpty.SpecName, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Name);

    [Fact]
    public void ContentType_HasNameId() =>
        Equal(CodeTypeWithSpecsEmpty.SpecGuid, ctDefManager.CreateTac<TCodeTypeWithSpecs>().NameId);

    [Fact]
    public void ContentType_HasScope() =>
        Equal(CodeTypeWithSpecsEmpty.SpecScope, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Scope);

    [Fact]
    public void ContentType_HasDescription() =>
        Equal(CodeTypeWithSpecsEmpty.SpecDescription, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Metadata.FirstModel<ContentTypeDetails>()!.Description);

    #endregion

}
