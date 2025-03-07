using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Builder.ClassesWithTypeDecoration;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Data.Builder;

[Startup(typeof(TestStartupEavCore))]
public class ContentTypeFactoryClassTests(ContentTypeFactory factory)
{
    private static string? GetDescription(IContentType type)
        => type.Metadata.Description?.Get<string>(nameof(ContentTypeDetails.Description));

    [Fact]
    public void Basic() => Assert.NotNull(factory);


    private T GetPropNoSpecsEmpty<T>(Func<IContentType, T> getFunc) => getFunc(factory.Create(typeof(TestTypeNoSpecsEmpty)));

    [Fact] public void Create_NoSpecs_Name() => Assert.Equal(nameof(TestTypeNoSpecsEmpty), GetPropNoSpecsEmpty(x => x.Name));

    [Fact] public void Create_NoSpecs_Scope() => Assert.Equal(Scopes.Default, GetPropNoSpecsEmpty(x => x.Scope));

    [Fact] public void Create_NoSpecs_NameId() => Assert.Equal(Guid.Empty.ToString(), GetPropNoSpecsEmpty(x => x.NameId));

    [Fact] public void Create_NoSpecs_AppId() => Assert.Equal(ContentTypeFactory.NoAppId, GetPropNoSpecsEmpty(x => x.AppId));

    [Fact] public void Create_NoSpecs_Description() => Assert.Null(GetDescription(factory.Create(typeof(TestTypeNoSpecsEmpty))));


    private T GetPropWithSpecsEmpty<T>(Func<IContentType, T> getFunc) => getFunc(factory.Create(typeof(TestTypeWithSpecsEmpty)));

    [Fact] public void Create_WithSpecs_Name() => Assert.Equal(TestTypeWithSpecsEmpty.SpecName, GetPropWithSpecsEmpty(x => x.Name));

    [Fact] public void Create_WithSpecs_Scope() => Assert.Equal(TestTypeWithSpecsEmpty.SpecScope, GetPropWithSpecsEmpty(x => x.Scope));

    [Fact] public void Create_WithSpecs_NameId() => Assert.Equal(TestTypeWithSpecsEmpty.SpecGuid, GetPropWithSpecsEmpty(x => x.NameId));

    [Fact] public void Create_WithSpecs_AppId() => Assert.Equal(ContentTypeFactory.NoAppId, GetPropWithSpecsEmpty(x => x.AppId));

    [Fact] public void Create_WithSpecs_Description() => Assert.Equal(TestTypeWithSpecsEmpty.SpecDescription, GetDescription(factory.Create(typeof(TestTypeWithSpecsEmpty))));

}