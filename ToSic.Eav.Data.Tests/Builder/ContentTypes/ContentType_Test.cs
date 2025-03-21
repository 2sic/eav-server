﻿using ToSic.Eav.Data.Build;

namespace ToSic.Eav.Data.Builder;

[Startup(typeof(StartupTestsEavCore))]
public class ContentType_Test(ContentTypeBuilder ctBuilder)
{
    [Fact]
    public void ContentType_GeneralTest()
    {
        var x = ctBuilder.CreateContentTypeTac(appId: -1, id: 0, name: "SomeName", scope: "TestScope");
        Assert.Equal("SomeName", x.Name);
        Assert.Equal("TestScope", x.Scope); // not set, should be blank

    }
}