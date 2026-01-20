using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.CodeContentTypes;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Builder;

[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypeFactoryClassTests(ContentTypeFactory factory)
{
    //private static string? GetDescription(IContentType type)
    //    => type.Metadata.Description?.Get<string>(nameof(ContentTypeDetails.Description));

    [Fact]
    public void Basic()
        => NotNull(factory);


    private T GetPropNoSpecsEmpty<T>(Func<IContentType, T> getFunc)
        => getFunc(factory.CreateTac<CodeTypeNoSpecsEmpty>());

    [Fact]
    public void Create_NoSpecs_Name()
        => Equal(nameof(CodeTypeNoSpecsEmpty), GetPropNoSpecsEmpty(x => x.Name));

    [Fact]
    public void Create_NoSpecs_Scope()
        => Equal(ScopeConstants.Default, GetPropNoSpecsEmpty(x => x.Scope));

    [Fact]
    public void Create_NoSpecs_NameId()
        => Equal(Guid.Empty.ToString(), GetPropNoSpecsEmpty(x => x.NameId));

    [Fact]
    public void Create_NoSpecs_AppId()
        => Equal(ContentTypeFactory.NoAppId, GetPropNoSpecsEmpty(x => x.AppId));

    //[Fact] public void Create_NoSpecs_Description() => Null(GetDescription(factory.Create(typeof(TestTypeNoSpecsEmpty))));


    private T GetPropWithSpecsEmpty<T>(Func<IContentType, T> getFunc)
        => getFunc(factory.CreateTac<CodeTypeWithSpecsEmpty>());

    [Fact]
    public void Create_WithSpecs_Name()
        => Equal(CodeTypeWithSpecsEmpty.SpecName, GetPropWithSpecsEmpty(x => x.Name));

    [Fact]
    public void Create_WithSpecs_Scope()
        => Equal(CodeTypeWithSpecsEmpty.SpecScope, GetPropWithSpecsEmpty(x => x.Scope));

    [Fact]
    public void Create_WithSpecs_NameId()
        => Equal(CodeTypeWithSpecsEmpty.SpecGuid, GetPropWithSpecsEmpty(x => x.NameId));

    [Fact]
    public void Create_WithSpecs_AppId()
        => Equal(ContentTypeFactory.NoAppId, GetPropWithSpecsEmpty(x => x.AppId));

    //[Fact] public void Create_WithSpecs_Description() => Equal(TestTypeWithSpecsEmpty.SpecDescription, GetDescription(factory.Create(typeof(TestTypeWithSpecsEmpty))));

}