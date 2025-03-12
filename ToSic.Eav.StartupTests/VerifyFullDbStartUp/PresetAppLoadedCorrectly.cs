using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Metadata;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.VerifyFullDbStartUp;

[Startup(typeof(StartupTestFullWithDb))]
public class PresetAppLoadedCorrectly(IAppReaderFactory appReaders) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{

    [Fact]
    public void PresetAppStateIdIsMinus42() =>
        Equal(Constants.PresetAppId, appReaders.GetSystemPresetTac().AppId);

    [Fact]
    public void IsHealthy() =>
        True(appReaders.GetSystemPresetTac().GetCache().IsHealthy);

    [Fact]
    public void PresetAppStateHasLotsOfData() => 
        True(appReaders.GetSystemPresetTac().List.Count > 100);

    [Fact]
    public void PresetAppStateHasNoteDecoratorContentTypeByName() => 
        NotNull(appReaders.GetSystemPresetTac().GetContentType(Decorators.NoteDecoratorName));

    [Fact]
    public void PresetAppStateHasNoteDecoratorContentTypeById() => 
        NotNull(appReaders.GetSystemPresetTac().GetContentType(Decorators.NoteDecoratorId));

    [Fact]
    public void PresetAppStateHasNotes() => 
        NotEmpty(appReaders.GetSystemPresetTac().List.OfType(Decorators.NoteDecoratorName));

    [Fact]
    public void PresetAppStateHasPickerDataSourceContentTypeById() => 
        NotNull(appReaders.GetSystemPresetTac().GetContentType(Decorators.IsPickerDataSourceDecoratorId));

}