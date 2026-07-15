using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Build.CodeContentTypes;
// ReSharper disable UnusedMember.Global

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Class** having no configuration.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfigNoneClass(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<CodeTypeNoSpecsClass>(ctDefManager);

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Record** having no configuration.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfigNoneRecord(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<CodeTypeNoSpecsRecord>(ctDefManager);

// ReSharper restore UnusedMember.Global

/// <summary>
/// Shared (abstract) tests for classes or records which are not configured (no attributes)
/// </summary>
public abstract class CodeCtFactoryConfigNone<TCodeTypeNoSpecs>(CodeContentTypesManager ctDefManager)
{
    [Fact]
    public void Attributes_NoSpec_Count()
        => Equal(4, ctDefManager.CreateTac<TCodeTypeNoSpecs>().Attributes.Count());
   
    
    [Fact]
    public void Attributes_NoSpec_NoVDecorator()
        => Null(ctDefManager.GetVirtualAttribDecorator(typeof(TCodeTypeNoSpecs)));
    
    
    [Theory]
    [InlineData(nameof(CodeTypeNoSpecsClass.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeNoSpecsClass.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeNoSpecsClass.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeNoSpecsClass.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => ctDefManager.CreateTac<TCodeTypeNoSpecs>().AssertAttributeDefinition(name, type);

    #region Inspect ContentType of generated data

    [Fact] 
    public void ContentType_IsNotNull()
        => NotNull(ctDefManager.CreateTac<TCodeTypeNoSpecs>());
    
    [Fact]
    public void ContentType_IsNotDynamic()
        => False(ctDefManager.CreateTac<TCodeTypeNoSpecs>().IsDynamic);
    
    [Fact]
    public void ContentType_HasRepositoryTypeCodeReflection() 
        => Equal(RepositoryTypes.CodeReflection, ctDefManager.CreateTac<TCodeTypeNoSpecs>().RepositoryType);
    
    [Fact]
    public void ContentType_DoesNotAlwaysShareConfiguration() 
        => False(ctDefManager.CreateTac<TCodeTypeNoSpecs>().AlwaysShareConfiguration);
    
    [Fact]
    public void ContentType_HasNullSysSettings() 
        => Null(ctDefManager.CreateTac<TCodeTypeNoSpecs>().SysSettings);

    #endregion

    #region ContentType Specs match what is expected

    [Fact]
    public void ContentType_NameIsFromClass()
        => Equal(typeof(TCodeTypeNoSpecs).Name, ctDefManager.CreateTac<TCodeTypeNoSpecs>().Name);

    [Fact]
    public void ContentType_HasNameId() =>
        Equal(Guid.Empty.ToString(), ctDefManager.CreateTac<TCodeTypeNoSpecs>().NameId);

    [Fact]
    public void ContentType_HasScope() =>
        Equal(ScopeConstants.Default, ctDefManager.CreateTac<TCodeTypeNoSpecs>().Scope);

    [Fact]
    public void ContentType_NoDetailsMetadata() =>
        Null(ctDefManager.CreateTac<TCodeTypeNoSpecs>().Metadata.FirstModel<ContentTypeDetails>());

    #endregion

}
