using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Build.CodeContentTypes;
// ReSharper disable UnusedMember.Global

[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfiguredInterface(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigured<ICodeTypeWithSpecsInterface>(ctDefManager);

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
    [InlineData(CodeTypeSpecsConstants.NameAttrSpecsNameModified, ValueTypes.String, true)]
    [InlineData(nameof(CodeTypeSpecsConstants.Url), ValueTypes.Hyperlink)]
    [InlineData(nameof(CodeTypeSpecsConstants.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeSpecsConstants.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeSpecsConstants.IsAlive), ValueTypes.Boolean, false, CodeTypeSpecsConstants.IsAliveDescription)]
    public void AssertAttributeWithSpec(string name, ValueTypes type, bool isTitle = false, string? description = default)
        => ctDefManager.CreateTac<TCodeTypeWithSpecs>().AssertAttributeDefinition(name, type, isTitle, description);
    
    /// <summary>
    /// Don't use properties which are private, internal or have the Ignore attribute
    /// </summary>
    /// <param name="name"></param>
    [Theory]
    [InlineData(nameof(CodeTypeSpecsConstants.IgnoreThis))]
    [InlineData(nameof(CodeTypeSpecsConstants.InternalProperty))]
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
        => Equal(CodeTypeSpecsConstants.SpecName, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Name);

    [Fact]
    public void ContentType_HasNameId() =>
        Equal(CodeTypeSpecsConstants.SpecGuid, ctDefManager.CreateTac<TCodeTypeWithSpecs>().NameId);

    [Fact]
    public void ContentType_HasScope() =>
        Equal(CodeTypeSpecsConstants.SpecScope, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Scope);

    [Fact]
    public void ContentType_HasDescription() =>
        Equal(CodeTypeSpecsConstants.SpecDescription, ctDefManager.CreateTac<TCodeTypeWithSpecs>().Metadata.FirstModel<ContentTypeDetails>()!.Description);

    #endregion

}
