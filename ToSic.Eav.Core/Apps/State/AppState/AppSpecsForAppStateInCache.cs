using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Sxc.Apps;

namespace ToSic.Eav.Apps.State;

/// <summary>
/// Special helper class to provide <see cref="IAppSpecs"/> for the AppState.
/// It's primary purpose is to provide a Configuration Object on demand, since the underlying entity will change from time to time.
/// </summary>
internal class AppSpecsForAppStateInCache(AppState appState): IAppSpecsWithStateAndCache
{
    public int ZoneId => appState.ZoneId;

    public int AppId => appState.AppId;

    public string NameId => appState.NameId;

    public string Name => appState.Name;

    public string Folder => appState.Folder;

    public PiggyBack PiggyBack => appState.PiggyBack;
    

    /// <summary>
    /// Create the configuration reader on demand, since the underlying Entity could change.
    /// </summary>
    public IAppConfiguration Configuration => new AppConfiguration(appState.SettingsInApp.AppConfiguration);

    IAppSpecs IHas<IAppSpecs>.Value => this;

    IAppSpecsWithState IHas<IAppSpecsWithState>.Value => this;

    IAppStateCache IAppSpecsWithStateAndCache.Cache => appState;

    IAppSpecsWithStateAndCache IHas<IAppSpecsWithStateAndCache>.Value => this;
}