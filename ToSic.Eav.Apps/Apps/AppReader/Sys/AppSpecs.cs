using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Metadata;
using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Apps.AppReader.Sys;

/// <summary>
/// Special helper class to provide <see cref="IAppSpecs"/> for the AppState.
/// It's primary purpose is to provide a Configuration Object on demand, since the underlying entity will change from time to time.
/// </summary>
internal class AppSpecs(AppState appState): IAppSpecs
{
    public int ZoneId => appState.ZoneId;

    public int AppId => appState.AppId;

    public string NameId => appState.NameId;

    public string Name => appState.Name ?? AppSpecConstants.ErrorAppNameNotLoaded;

    public string Folder => appState.Folder ?? AppSpecConstants.ErrorAppFolderNotLoaded;

    public string RuntimeKey => appState.RuntimeKey;

    public PiggyBack PiggyBack => appState.PiggyBack;
    

    /// <summary>
    /// Create the configuration reader on demand, since the underlying Entity could change.
    /// </summary>
    // public IAppConfiguration Configuration => new AppConfiguration(appState.SettingsInApp.AppConfiguration!);
    public IAppConfiguration Configuration => appState.SettingsInApp.AppConfiguration.ToModel<AppConfiguration>(skipTypeCheck: true, /*nullIfNull: false,*/ nullHandling: ModelNullHandling.PreferModelForce)!;

    public IMetadata Metadata => appState.Metadata;

    public IAppStateMetadata Settings => appState.SettingsInApp;

    public IAppStateMetadata Resources => appState.ResourcesInApp;

}
