using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// Verify that the specs given to a PoCo without an attribute match what is expected.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryNoConfigDefaultSpecs(CodeContentTypesManager ctDefManager)
{
    private IContentType GetCtNoSpecsEmpty()
        => ctDefManager.CreateTac<CodeTypeNoSpecsEmpty>();
    
    private IContentType GetCtWithSpecsEmpty()
        => ctDefManager.CreateTac<CodeTypeWithSpecsEmpty>();

    [Theory]
    [InlineData(nameof(CodeTypeNoSpecsEmpty), typeof(CodeTypeNoSpecsEmpty))]
    [InlineData(CodeTypeWithSpecsEmpty.SpecName, typeof(CodeTypeWithSpecsEmpty))]
    public void Name(string expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).Name);

    [Fact]
    public void Create_NoSpecs_Name()
        => Equal(nameof(CodeTypeNoSpecsEmpty), GetCtNoSpecsEmpty().Name);
    [Fact]
    public void Create_NoSpecs_Scope()
        => Equal(ScopeConstants.Default, GetCtNoSpecsEmpty().Scope);

    [Fact]
    public void Create_NoSpecs_NameId()
        => Equal(Guid.Empty.ToString(), GetCtNoSpecsEmpty().NameId);

    [Fact]
    public void Create_NoSpecs_AppId()
        => Equal(CodeContentTypesManager.NoAppId, GetCtNoSpecsEmpty().AppId);



    
    
    
    
    [Fact]
    public void Create_WithSpecs_Name()
        => Equal(CodeTypeWithSpecsEmpty.SpecName, GetCtWithSpecsEmpty().Name);

    [Fact]
    public void Create_WithSpecs_Scope()
        => Equal(CodeTypeWithSpecsEmpty.SpecScope, GetCtWithSpecsEmpty().Scope);

    [Fact]
    public void Create_WithSpecs_NameId()
        => Equal(CodeTypeWithSpecsEmpty.SpecGuid, GetCtWithSpecsEmpty().NameId);

    [Fact]
    public void Create_WithSpecs_AppId()
        => Equal(CodeContentTypesManager.NoAppId, GetCtWithSpecsEmpty().AppId);
}