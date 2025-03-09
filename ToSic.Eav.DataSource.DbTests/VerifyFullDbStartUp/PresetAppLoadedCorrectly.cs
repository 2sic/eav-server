using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;
using ToSic.Eav.RelationshipTests;
using ToSic.Eav.Testing;

namespace ToSic.Eav.DataSource.DbTests.VerifyFullDbStartUp;

[Startup(typeof(TestStartupFullWithDb))]
public class PresetAppLoadedCorrectly(IAppReaderFactory appReaders) : IClassFixture<FullDbFixtureScenarioBasic>
{

    [Fact]
    public void PresetAppStateIdIsMinus42() =>
        Equal(Constants.PresetAppId, appReaders.GetSystemPreset().AppId);

    [Fact]
    public void PresetAppStateHasLotsOfData() => 
        True(appReaders.GetSystemPreset().List.Count > 100);

    [Fact]
    public void PresetAppStateHasNoteDecoratorContentTypeByName() => 
        NotNull(appReaders.GetSystemPreset().GetContentType(Decorators.NoteDecoratorName));

    [Fact]
    public void PresetAppStateHasNoteDecoratorContentTypeById() => 
        NotNull(appReaders.GetSystemPreset().GetContentType(Decorators.NoteDecoratorId));

    [Fact]
    public void PresetAppStateHasNotes() => 
        NotEmpty(appReaders.GetSystemPreset().List.OfType(Decorators.NoteDecoratorName));

    [Fact]
    public void PresetAppStateHasPickerDataSourceContentTypeById() => 
        NotNull(appReaders.GetSystemPreset().GetContentType(Decorators.IsPickerDataSourceDecoratorId));

    [Fact]
    public void GetContentTypeOnNormalAppFailsInNet9AskSTV() => 
        NotNull(appReaders.Get(RelationshipTestSpecs.AppIdentity).GetContentType(Decorators.IsPickerDataSourceDecoratorId));

}