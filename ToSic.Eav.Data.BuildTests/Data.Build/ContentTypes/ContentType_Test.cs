namespace ToSic.Eav.Data.Build.ContentTypes;

[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentType_Test(ContentTypeBuilder ctBuilder)
{
    [Fact]
    public void ContentType_GeneralTest()
    {
        var x = ctBuilder.CreateContentTypeTac(appId: -1, id: 0, name: "SomeName", scope: "TestScope");
        Equal("SomeName", x.Name);
        Equal("TestScope", x.Scope); // not set, should be blank

    }
}