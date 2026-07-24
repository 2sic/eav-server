using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsNone;
// ReSharper disable UnusedMember.Global

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Class** having no configuration.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypesFromCodeBuildSpecsNoneInterface(ContentTypesFromCodeManager ctDefManager)
    : ContentTypesFromCodeBuildSpecsNone<ICodeTypeSpecsNoInterface>(ctDefManager);


/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Class** having no configuration.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypesFromCodeBuildSpecsNoneClass(ContentTypesFromCodeManager ctDefManager)
    : ContentTypesFromCodeBuildSpecsNone<CodeTypeSpecsNoClass>(ctDefManager);

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Record** having no configuration.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypesFromCodeBuildSpecsNoneRecord(ContentTypesFromCodeManager ctDefManager)
    : ContentTypesFromCodeBuildSpecsNone<CodeTypeSpecsNoRecord>(ctDefManager);

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Record** having no configuration.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypesFromCodeBuildSpecsNoneAnonymous(ContentTypesFromCodeManager ctDefManager)
    : ContentTypesFromCodeBuildSpecsNone<object>(ctDefManager, useAnonymous: true);

// ReSharper restore UnusedMember.Global




/// <summary>
/// Shared (abstract) tests for classes or records which are not configured (no attributes)
/// </summary>
public abstract class ContentTypesFromCodeBuildSpecsNone<TCodeTypeNoSpecs>(ContentTypesFromCodeManager ctDefManager, bool useAnonymous = false)
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
    [InlineData(nameof(CodeTypeSpecsNoClass.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeSpecsNoClass.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeSpecsNoClass.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeSpecsNoClass.IsAlive), ValueTypes.Boolean)]
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
            ? ContentTypesFromCodeBuilder.AnonymousTypeName
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
