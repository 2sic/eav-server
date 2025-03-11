using ToSic.Eav.Apps;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.Configuration;

// TODO: These tests seem to not have worked for a long time
// Probably the setup isn't happening first.
// It appears that there should be some override settings which should be used as well, which are not applied here
// Needs some time to restore functionality

[Startup(typeof(StartupTestFullWithDb))]
public class GlobalConfigurationOverride(IAppReaderFactory appReaderFactory)
    // the fixture will also load the resources
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    public record TestData(string Name, Guid Guid, bool Expected);

    public static TheoryData<TestData> GenerateTests =>
    [
        new("Fancybox4", new("3356ad17-91ce-4814-83c1-9f527697391a"), false),
        new("Fancybox3", new("4b9ef331-480a-4a38-86f1-a580f8345677"), true)
    ];

    private const string HtmlField = "Html";
    private const string TestStringInOverrideFancybox3 = "test-is-override";

    [Theory]
    [MemberData(nameof(GenerateTests))]
    public void ResourceExistsOnce(TestData specs) =>
        Single(GetEntitiesOfGuid(specs.Guid));

    // TODO: @STV - this seems to fail, it appears that the normal data isn't loaded, only system-custom ?
    /// <summary>
    /// This is quite a complex test
    /// - There is an entity in App_Data/system-custom with the same guid as the fancybox3 WebResource
    /// - It has an additional string containing "test-is-override"
    /// - On load, it should _replace_ the original item
    /// - and make sure it's used instead
    /// </summary>
    [Theory]
    [MemberData(nameof(GenerateTests))]
    public void ResourceHtmlContainsText(TestData specs)
    {
        var html = GetEntitiesOfGuid(specs.Guid).First().GetTac<string>(HtmlField);
        Equal(specs.Expected, html.Contains(TestStringInOverrideFancybox3));
    }

    private List<IEntity> GetEntitiesOfGuid(Guid resourceGuid)
    {
        var primaryApp = appReaderFactory.GetSystemPreset();

        // Verify there is only one with this guid
        var entities = primaryApp.List
            .Where(e => e.EntityGuid == resourceGuid)
            .ToList();
        return entities;
    }
}