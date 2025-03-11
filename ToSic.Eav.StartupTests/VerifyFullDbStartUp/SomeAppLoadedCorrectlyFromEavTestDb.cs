using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Metadata;
using ToSic.Eav.Testing;

namespace ToSic.Eav.DataSource.DbTests.VerifyFullDbStartUp;

[Startup(typeof(StartupTestFullWithDb))]
public class SomeAppLoadedCorrectlyFromEavTestDb(IAppReaderFactory appReaders) : IClassFixture<FullDbFixtureScenarioBasic>
{
    /// <summary>
    /// This is the same App as used for Relationship tests, but what we're testing here is not specific to that app
    /// </summary>
    private static IAppIdentity AppIdentity = new AppIdentity(2, 3);

    [Fact]
    public void GetApp() => 
        NotNull(appReaders.Get(AppIdentity));

    [Fact]
    public void IsHealthy() =>
        True(appReaders.Get(AppIdentity).GetCache().IsHealthy);

    [Fact]
    public void GetContentTypeOnNormalAppFailsInNet9AskSTV() => 
        NotNull(appReaders.Get(AppIdentity).GetContentType(Decorators.IsPickerDataSourceDecoratorId));

}