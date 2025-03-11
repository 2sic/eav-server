using ToSic.Eav.Apps;
using ToSic.Eav.Testing;

namespace ToSic.Eav.Configuration;

// TODO: These tests seem to not have worked for a long time
// Probably the setup isn't happening first.
// It appears that there should be some override settings which should be used as well, which are not applied here
// Needs some time to restore functionality

[Startup(typeof(StartupTestFullWithDb))]
public class GlobalConfigurationOverride(IAppReaderFactory appReaderFactory) : IClassFixture<FullDbFixtureScenarioBasic>
{
    //public DataOverrideTest() => Build<EavSystemLoader>().LoadLicenseAndFeatures();

    private static Guid fancybox4ItemGuid = new("3356ad17-91ce-4814-83c1-9f527697391a");
    private static Guid fancybox3ItemGuid = new("4b9ef331-480a-4a38-86f1-a580f8345677");

    private const string htmlField = "Html";
    private const string testString = "test-is-override";

    // TODO: @STV - this seems to fail, it appears that the normal data isn't loaded, only system-custom ?
    [Fact]
    public void TestNormalFancybox4() => TestWebResourcesExistsOnceAndMayHaveValue(fancybox4ItemGuid, false);

    /// <summary>
    /// This is quite a complex test
    /// - There is an entity in App_Data/system-custom with the same guid as the fancybox3 WebResource
    /// - It has an additional string containing "test-is-override"
    /// - On load, it should _replace_ the original item
    /// - and make sure it's used instead
    /// </summary>
    [Fact]
    public void TestOverrideFancybox3() => TestWebResourcesExistsOnceAndMayHaveValue(fancybox3ItemGuid, true);


    private void TestWebResourcesExistsOnceAndMayHaveValue(Guid guid, bool expected)
    {
        var primaryApp = appReaderFactory.GetSystemPreset();

        // Verify there is only one with this guid
        var entities = primaryApp.List.Where(e => e.EntityGuid == guid);
        Single(entities);

        var entity = primaryApp.List.One(guid);
        var html = entity.GetTac<string>(htmlField);

        Equal(expected, html.Contains(testString));
    }


}