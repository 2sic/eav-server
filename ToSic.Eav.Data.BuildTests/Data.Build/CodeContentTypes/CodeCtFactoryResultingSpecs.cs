using ToSic.Eav.Data.Build.CodeContentTypes.SpecsNone;
using ToSic.Eav.Data.Build.CodeContentTypes.SpecsYes;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Sys.TestHelpers.Assembly;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// Verify that the specs given to a PoCo without an attribute match what is expected.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryResultingSpecs(ContentTypesFromCodeManager ctDefManager)
{
    [Theory]
    [InlineData(false, typeof(CodeTypeSpecsNoEmpty))]
    [InlineData(true, typeof(CodeTypeSpecsYesEmpty))]
    public void IsConfigured(bool expected, Type type)
        => Equal(expected, ctDefManager.IsConfiguredTac(type));


    [Theory]
    [InlineData(nameof(CodeTypeSpecsNoEmpty), typeof(CodeTypeSpecsNoEmpty))]
    [InlineData(CodeTypeSpecsConstants.SpecName, typeof(CodeTypeSpecsYesEmpty))]
    public void Name(string expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).Name);
    
    

    [Theory]
    [InlineData(ScopeConstants.Default, typeof(CodeTypeSpecsNoEmpty))]
    [InlineData(CodeTypeSpecsConstants.SpecScope, typeof(CodeTypeSpecsYesEmpty))]
    public void Scope(string expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).Scope);
    
    
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", typeof(CodeTypeSpecsNoEmpty))]
    [InlineData(CodeTypeSpecsConstants.SpecGuid, typeof(CodeTypeSpecsYesEmpty))]
    public void NameId(string expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).NameId);

    
    [Theory]
    [InlineData(ContentTypesFromCodeManager.NoAppId, typeof(CodeTypeSpecsNoEmpty))]
    [InlineData(ContentTypesFromCodeManager.NoAppId, typeof(CodeTypeSpecsYesEmpty))]
    public void AppId(int expected, Type type)
        => Equal(expected, ctDefManager.CreateTac(type).AppId);

    /// <summary>
    /// Configured types have a repositoryType of CodeConfiguration, while non-configured types have a repositoryType of CodeReflection.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="typeName"></param>
    [Theory]
    [InlineData(RepositoryTypes.CodeReflection, nameof(CodeTypeSpecsNoEmpty))]
    [InlineData(RepositoryTypes.CodeConfiguration, nameof(CodeTypeSpecsYesEmpty))]
    public void RepositoryType(RepositoryTypes expected, string typeName)
        => Equal(expected, ctDefManager.CreateTac(typeof(CodeTypeSpecsNoEmpty).Assembly.GetTypeFromName(typeName)).RepositoryType);


}