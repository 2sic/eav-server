using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.Data.Build.ContentTypes;

[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentType_Test(ContentTypeTypeAssembler ctAssembler)
{
    [Fact]
    public void ContentType_GeneralTest()
    {
        var x = ctAssembler.CreateContentTypeTac(appId: -1, id: 0, name: "SomeName", scope: "TestScope");
        Equal("SomeName", x.Name);
        Equal("TestScope", x.Scope); // not set, should be blank

    }
}