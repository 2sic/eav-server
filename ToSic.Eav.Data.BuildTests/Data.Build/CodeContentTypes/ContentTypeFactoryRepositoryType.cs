using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Sys.TestHelpers.Assembly;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypeFactoryRepositoryType(CodeContentTypesManager ctDefFactory)
{

    [Theory]
    [InlineData(RepositoryTypes.CodeReflection, nameof(CodeTypeNoSpecsEmpty))]
    [InlineData(RepositoryTypes.CodeConfiguration, nameof(CodeTypeWithSpecsEmpty))]
    public void Create_NoSpecs_RepositoryType(RepositoryTypes expected, string typeName)
        => Equal(expected, ctDefFactory.CreateTac(typeof(CodeTypeNoSpecsEmpty).Assembly.GetTypeFromName(typeName)).RepositoryType);


}