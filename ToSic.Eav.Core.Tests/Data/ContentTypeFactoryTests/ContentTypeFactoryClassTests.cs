using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;

namespace ToSic.Eav.Core.Tests.Data.ContentTypeFactoryTests;

[TestClass]
public class ContentTypeFactoryClassTests: ContentTypeFactoryTestsBase
{
    private static string GetDescription(IContentType type)
        => type.Metadata.Description?.Get<string>(nameof(ContentTypeDetails.Description));

    [TestMethod]
    public void Basic() => Assert.IsNotNull(Factory());


    private T GetPropNoSpecsEmpty<T>(Func<IContentType, T> getFunc) => getFunc(Factory().Create(typeof(TestTypeNoSpecsEmpty)));

    [TestMethod] public void Create_NoSpecs_Name() => Assert.AreEqual(nameof(TestTypeNoSpecsEmpty), GetPropNoSpecsEmpty(x => x.Name));

    [TestMethod] public void Create_NoSpecs_Scope() => Assert.AreEqual(Scopes.Default, GetPropNoSpecsEmpty(x => x.Scope));

    [TestMethod] public void Create_NoSpecs_NameId() => Assert.AreEqual(Guid.Empty.ToString(), GetPropNoSpecsEmpty(x => x.NameId));

    [TestMethod] public void Create_NoSpecs_AppId() => Assert.AreEqual(ContentTypeFactory.NoAppId, GetPropNoSpecsEmpty(x => x.AppId));

    [TestMethod] public void Create_NoSpecs_Description() => Assert.AreEqual(null, GetDescription(Factory().Create(typeof(TestTypeNoSpecsEmpty))));


    private T GetPropWithSpecsEmpty<T>(Func<IContentType, T> getFunc) => getFunc(Factory().Create(typeof(TestTypeWithSpecsEmpty)));

    [TestMethod] public void Create_WithSpecs_Name() => Assert.AreEqual(TestTypeWithSpecsEmpty.SpecName, GetPropWithSpecsEmpty(x => x.Name));

    [TestMethod] public void Create_WithSpecs_Scope() => Assert.AreEqual(TestTypeWithSpecsEmpty.SpecScope, GetPropWithSpecsEmpty(x => x.Scope));

    [TestMethod] public void Create_WithSpecs_NameId() => Assert.AreEqual(TestTypeWithSpecsEmpty.SpecGuid, GetPropWithSpecsEmpty(x => x.NameId));

    [TestMethod] public void Create_WithSpecs_AppId() => Assert.AreEqual(ContentTypeFactory.NoAppId, GetPropWithSpecsEmpty(x => x.AppId));

    [TestMethod] public void Create_WithSpecs_Description() => Assert.AreEqual(TestTypeWithSpecsEmpty.SpecDescription, GetDescription(Factory().Create(typeof(TestTypeWithSpecsEmpty))));

}