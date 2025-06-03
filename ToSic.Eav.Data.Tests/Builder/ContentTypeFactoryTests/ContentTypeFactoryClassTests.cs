using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Builder.ClassesWithTypeDecoration;

namespace ToSic.Eav.Data.Builder;

[Startup(typeof(StartupTestsEavCore))]
public class ContentTypeFactoryClassTests(ContentTypeFactory factory)
{
    //private static string? GetDescription(IContentType type)
    //    => type.Metadata.Description?.Get<string>(nameof(ContentTypeDetails.Description));

    [Fact]
    public void Basic() => NotNull(factory);


    private T GetPropNoSpecsEmpty<T>(Func<IContentType, T> getFunc) => getFunc(factory.Create(typeof(TestTypeNoSpecsEmpty)));

    [Fact] public void Create_NoSpecs_Name() => Equal(nameof(TestTypeNoSpecsEmpty), GetPropNoSpecsEmpty(x => x.Name));

    [Fact] public void Create_NoSpecs_Scope() => Equal(ScopeConstants.Default, GetPropNoSpecsEmpty(x => x.Scope));

    [Fact] public void Create_NoSpecs_NameId() => Equal(Guid.Empty.ToString(), GetPropNoSpecsEmpty(x => x.NameId));

    [Fact] public void Create_NoSpecs_AppId() => Equal(ContentTypeFactory.NoAppId, GetPropNoSpecsEmpty(x => x.AppId));

    //[Fact] public void Create_NoSpecs_Description() => Null(GetDescription(factory.Create(typeof(TestTypeNoSpecsEmpty))));


    private T GetPropWithSpecsEmpty<T>(Func<IContentType, T> getFunc) => getFunc(factory.Create(typeof(TestTypeWithSpecsEmpty)));

    [Fact] public void Create_WithSpecs_Name() => Equal(TestTypeWithSpecsEmpty.SpecName, GetPropWithSpecsEmpty(x => x.Name));

    [Fact] public void Create_WithSpecs_Scope() => Equal(TestTypeWithSpecsEmpty.SpecScope, GetPropWithSpecsEmpty(x => x.Scope));

    [Fact] public void Create_WithSpecs_NameId() => Equal(TestTypeWithSpecsEmpty.SpecGuid, GetPropWithSpecsEmpty(x => x.NameId));

    [Fact] public void Create_WithSpecs_AppId() => Equal(ContentTypeFactory.NoAppId, GetPropWithSpecsEmpty(x => x.AppId));

    //[Fact] public void Create_WithSpecs_Description() => Equal(TestTypeWithSpecsEmpty.SpecDescription, GetDescription(factory.Create(typeof(TestTypeWithSpecsEmpty))));

}