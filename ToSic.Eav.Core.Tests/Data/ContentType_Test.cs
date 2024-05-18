using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data.Build;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Data;

[TestClass]
public class ContentType_Test: TestBaseEavCore
{
    private const int AppIdX = -1;
    [TestMethod]
    public void ContentType_GeneralTest()
    {
        var x = GetService<ContentTypeBuilder>()
            .TestCreate(appId: AppIdX, id: 0, name: "SomeName", scope: "TestScope");
        Assert.AreEqual("SomeName", x.Name);
        Assert.AreEqual("TestScope", x.Scope); // not set, should be blank

    }
}