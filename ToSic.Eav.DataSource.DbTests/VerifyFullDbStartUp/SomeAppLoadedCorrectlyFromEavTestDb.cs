using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.DataSource.DbTests.RelationshipTests;
using ToSic.Eav.Metadata;
using ToSic.Eav.Testing;

namespace ToSic.Eav.DataSource.DbTests.VerifyFullDbStartUp;

[Startup(typeof(StartupTestFullWithDb))]
public class SomeAppLoadedCorrectlyFromEavTestDb(IAppReaderFactory appReaders) : IClassFixture<FullDbFixtureScenarioBasic>
{

    [Fact]
    public void GetApp() => 
        NotNull(appReaders.Get(RelationshipTestSpecs.AppIdentity));

    [Fact]
    public void IsHealthy() =>
        True(appReaders.Get(RelationshipTestSpecs.AppIdentity).GetCache().IsHealthy);

    [Fact]
    public void GetContentTypeOnNormalAppFailsInNet9AskSTV() => 
        NotNull(appReaders.Get(RelationshipTestSpecs.AppIdentity).GetContentType(Decorators.IsPickerDataSourceDecoratorId));

}