using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Sys.TestHelpers.Assembly;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypeFactoryIsConfigured(CodeContentTypesManager ctDefFactory)
{
    [Fact]
    public void NoSpecsIsNotConfigured()
        => False(ctDefFactory.IsConfiguredTac(typeof(CodeTypeNoSpecsEmpty)));

    [Fact]
    public void SpecsIsConfigured()
        => True(ctDefFactory.IsConfiguredTac(typeof(CodeTypeWithSpecsEmpty)));

    /// <summary>
    /// Configured types have a repositoryType of CodeConfiguration, while non-configured types have a repositoryType of CodeReflection.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="typeName"></param>
    [Theory]
    [InlineData(RepositoryTypes.CodeReflection, nameof(CodeTypeNoSpecsEmpty))]
    [InlineData(RepositoryTypes.CodeConfiguration, nameof(CodeTypeWithSpecsEmpty))]
    public void Create_NoSpecs_RepositoryType(RepositoryTypes expected, string typeName)
        => Equal(expected, ctDefFactory.CreateTac(typeof(CodeTypeNoSpecsEmpty).Assembly.GetTypeFromName(typeName)).RepositoryType);

}