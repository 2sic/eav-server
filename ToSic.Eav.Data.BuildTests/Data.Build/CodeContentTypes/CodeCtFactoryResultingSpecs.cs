using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Sys.TestHelpers.Assembly;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// Verify that the specs given to a PoCo without an attribute match what is expected.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryResultingSpecs(CodeContentTypesManager ctDefManager)
{
    [Theory]
    [InlineData(false, typeof(CodeTypeNoSpecsEmpty))]
    [InlineData(true, typeof(CodeTypeWithSpecsEmpty))]
    public void IsConfigured(bool expected, Type type)
        => Equal(expected, ctDefManager.IsConfiguredTac(type));


    [Theory]
    [InlineData(nameof(CodeTypeNoSpecsEmpty), typeof(CodeTypeNoSpecsEmpty))]
    [InlineData(CodeTypeSpecsConstants.SpecName, typeof(CodeTypeWithSpecsEmpty))]
    public void Name(string expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).Name);
    
    

    [Theory]
    [InlineData(ScopeConstants.Default, typeof(CodeTypeNoSpecsEmpty))]
    [InlineData(CodeTypeSpecsConstants.SpecScope, typeof(CodeTypeWithSpecsEmpty))]
    public void Scope(string expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).Scope);
    
    
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", typeof(CodeTypeNoSpecsEmpty))]
    [InlineData(CodeTypeSpecsConstants.SpecGuid, typeof(CodeTypeWithSpecsEmpty))]
    public void NameId(string expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).NameId);

    
    [Theory]
    [InlineData(CodeContentTypesManager.NoAppId, typeof(CodeTypeNoSpecsEmpty))]
    [InlineData(CodeContentTypesManager.NoAppId, typeof(CodeTypeWithSpecsEmpty))]
    public void AppId(int expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).AppId);

    /// <summary>
    /// Configured types have a repositoryType of CodeConfiguration, while non-configured types have a repositoryType of CodeReflection.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="typeName"></param>
    [Theory]
    [InlineData(RepositoryTypes.CodeReflection, nameof(CodeTypeNoSpecsEmpty))]
    [InlineData(RepositoryTypes.CodeConfiguration, nameof(CodeTypeWithSpecsEmpty))]
    public void RepositoryType(RepositoryTypes expected, string typeName)
        => Equal(expected, ctDefManager.CreateTac(typeof(CodeTypeNoSpecsEmpty).Assembly.GetTypeFromName(typeName)).RepositoryType);


}