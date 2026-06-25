using ToSic.Eav.Data.Build.Sys;

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

}