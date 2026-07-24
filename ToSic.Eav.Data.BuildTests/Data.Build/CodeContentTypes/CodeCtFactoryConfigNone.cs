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
public class CodeCtFactoryConfigNoneInterface(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<ICodeTypeNoSpecsInterface>(ctDefManager);


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

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Record** having no configuration.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfigNoneAnonymous(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<object>(ctDefManager, useAnonymous: true);

// ReSharper restore UnusedMember.Global




/// <summary>
/// Shared (abstract) tests for classes or records which are not configured (no attributes)
/// </summary>
public abstract class CodeCtFactoryConfigNone<TCodeTypeNoSpecs>(CodeContentTypesManager ctDefManager, bool useAnonymous = false)
{
    /// <summary>
    /// Central place to get/create the content type.
    /// Must be centralized, as we also have a test scenario with anonymous
    /// </summary>
    /// <returns></returns>
    private IContentType GetCurrentContentType()
    {
        if (!useAnonymous)
            return ctDefManager.CreateTac<TCodeTypeNoSpecs>();

        var anonWithSimilarSignature = new
        {
            Id = 0,
            Name = "",
            Age = 0,
            BirthDate = new DateTime(),
            IsAlive = false,
        };
        return ctDefManager.CreateTac(anonWithSimilarSignature.GetType());
    }

    [Fact]
    public void Attributes_NoSpec_Count()
        => Equal(4, GetCurrentContentType().Attributes.Count());
   
    
    [Fact]
    public void Attributes_NoSpec_NoVDecorator()
        => Null(ctDefManager.GetVirtualAttribDecorator(typeof(TCodeTypeNoSpecs)));
    
    
    [Theory]
    [InlineData(nameof(CodeTypeNoSpecsClass.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeNoSpecsClass.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeNoSpecsClass.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeNoSpecsClass.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => GetCurrentContentType().AssertAttributeDefinition(name, type);

    #region Inspect ContentType of generated data

    [Fact] 
    public void ContentType_IsNotNull()
        => NotNull(GetCurrentContentType());
    
    [Fact]
    public void ContentType_IsNotDynamic()
        => False(GetCurrentContentType().IsDynamic);
    
    [Fact]
    public void ContentType_HasRepositoryTypeCodeReflection() 
        => Equal(RepositoryTypes.CodeReflection, GetCurrentContentType().RepositoryType);
    
    [Fact]
    public void ContentType_DoesNotAlwaysShareConfiguration() 
        => False(GetCurrentContentType().AlwaysShareConfiguration);
    
    [Fact]
    public void ContentType_HasNullSysSettings() 
        => Null(GetCurrentContentType().SysSettings);

    #endregion

    #region ContentType Specs match what is expected

    [Fact]
    public void ContentType_NameIsFromClass()
    {
        var expected = useAnonymous
            ? CodeContentTypeBuilder.AnonymousTypeName
            : typeof(TCodeTypeNoSpecs).Name;
        Equal(expected, GetCurrentContentType().Name);
    }

    [Fact]
    public void ContentType_HasNameId() =>
        Equal(Guid.Empty.ToString(), GetCurrentContentType().NameId);

    [Fact]
    public void ContentType_HasScope() =>
        Equal(ScopeConstants.Default, GetCurrentContentType().Scope);

    [Fact]
    public void ContentType_NoDetailsMetadata() =>
        Null(GetCurrentContentType().Metadata.FirstModel<ContentTypeDetails>());

    #endregion

}
