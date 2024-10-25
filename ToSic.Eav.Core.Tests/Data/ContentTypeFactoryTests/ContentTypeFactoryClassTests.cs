using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Data.ContentTypeFactoryTests;

[TestClass]
public class ContentTypeFactoryClassTests: TestBaseEavCore
{
    private ContentTypeFactory Factory() => GetService<ContentTypeFactory>();

    [TestMethod]
    public void Basic() => Assert.IsNotNull(Factory());

    [TestMethod]
    public void Create_NoSpecs()
    {
        var x = Factory().Create(typeof(TestTypeBasic));
        Assert.AreEqual(nameof(TestTypeBasic), x.Name);
        Assert.AreEqual(Scopes.Default, x.Scope);
        Assert.AreEqual(Guid.Empty.ToString(), x.NameId);
        Assert.AreEqual(ContentTypeFactory.NoAppId, x.AppId);
        Assert.AreEqual(null, GetDescription(x));
    }

    [TestMethod]
    public void Create_WithSpecs()
    {
        var x = Factory().Create(typeof(TestTypeWithSpecs));
        Assert.AreEqual(TestTypeWithSpecs.SpecName, x.Name);
        Assert.AreEqual(TestTypeWithSpecs.SpecScope, x.Scope);
        Assert.AreEqual(TestTypeWithSpecs.SpecGuid, x.NameId);
        Assert.AreEqual(ContentTypeFactory.NoAppId, x.AppId);
        Assert.AreEqual(TestTypeWithSpecs.SpecDescription, GetDescription(x));
    }

    private static string GetDescription(IContentType type) => type.Metadata.Description?.Get<string>(nameof(ContentTypeDetails.Description));
}