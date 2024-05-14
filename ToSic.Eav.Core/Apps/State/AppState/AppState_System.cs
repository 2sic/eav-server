namespace ToSic.Eav.Apps.State;

partial class AppState
{
    [PrivateApi]
    internal AppStateMetadata SettingsInApp => _settingsInApp ??= new(this, AppStackConstants.Settings);
    private AppStateMetadata _settingsInApp;


    [PrivateApi]
    internal AppStateMetadata ResourcesInApp => _resourcesInApp ??= new(this, AppStackConstants.Resources);
    private AppStateMetadata _resourcesInApp;

    [PrivateApi]
    AppStateMetadata IAppStateCache.ThingInApp(AppThingsToStack target) =>
        target == AppThingsToStack.Settings ? SettingsInApp : ResourcesInApp;
}