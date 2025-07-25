﻿using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.VerifyFullDbStartUp;

[Startup(typeof(StartupTestsApps))]
public class SomeAppLoadedCorrectlyFromEavTestDb(IAppReaderFactory appReaders) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    /// <summary>
    /// This is the same App as used for Relationship tests, but what we're testing here is not specific to that app
    /// </summary>
    private static IAppIdentity AppIdentity = new AppIdentity(2, 3);

    [Fact]
    public void GetApp() => 
        NotNull(appReaders.GetTac(AppIdentity));

    [Fact]
    public void IsHealthy() =>
        True(appReaders.GetTac(AppIdentity).GetCache().IsHealthy);

    [Fact]
    public void GetContentTypeOnNormalAppFailsInNet9AskSTV() => 
        NotNull(appReaders.GetTac(AppIdentity).TryGetContentTypeTac(KnownDecorators.IsPickerDataSourceDecoratorId));

}