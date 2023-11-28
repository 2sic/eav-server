using System.Collections.Generic;
using ToSic.Eav.Apps.Reader;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps;

partial class App: IHasPermissions
{
    #region Metadata and Permission Accessors

    /// <inheritdoc />
    public IMetadataOf Metadata { get; private set; }

    /// <summary>
    /// Permissions of this app
    /// </summary>
    public IEnumerable<Permission> Permissions => Metadata.Permissions;

    #endregion

    #region Settings, Config, Metadata
    protected IEntity AppConfiguration;
    protected IEntity AppSettings;
    protected IEntity AppResources;

    /// <summary>
    /// Assign all kinds of metadata / resources / settings (App-Mode only)
    /// </summary>
    protected void InitializeResourcesSettingsAndMetadata()
    {
        var l = Log.Fn();
        var appState = AppStateInt;
        Metadata = appState.Metadata;

        // Get the content-items describing various aspects of this app
        AppResources = appState.ResourcesInApp.MetadataItem;
        AppSettings = appState.SettingsInApp.MetadataItem;
        AppConfiguration = appState.SettingsInApp.AppConfiguration;
        // in some cases these things may be null, if the app was created not allowing side-effects
        // This can usually happen when new apps are being created
        l.A($"HasResources: {AppResources != null}, HasSettings: {AppSettings != null}, HasConfiguration: {AppConfiguration != null}");

        // resolve some values for easier access
        Name = appState.Name ?? Constants.ErrorAppName;
        Folder = appState.Folder ?? Constants.ErrorAppName;

        Hidden = AppConfiguration?.Value<bool>(AppLoadConstants.FieldHidden) ?? false;
        l.Done($"Name: {Name}, Folder: {Folder}, Hidden: {Hidden}");
    }
    #endregion

    [PublicApi]
    public AppState AppState => AppStateInt.AppState; // _appState ??= Services.AppStates.Get(this);
    // private AppState _appState;

    [PrivateApi] public IAppState AppStateWIP => AppStateInt;

    protected internal IAppStateInternal AppStateInt => _appStateReader ??= Services.AppStates.GetReaderInternalOrNull(this);
    private IAppStateInternal _appStateReader;
}