using ToSic.Eav.Apps.State;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Internal;

partial class EavApp: IHasPermissions
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
        // in some cases these things may be null, if the app was created not allowing side-effects
        // This can usually happen when new apps are being created
        l.A($"HasResources: {AppResources != null}, HasSettings: {AppSettings != null}, HasConfiguration: {AppStateInt.ConfigurationEntity != null}");

        // resolve some values for easier access
        Name = appState.Name ?? Constants.ErrorAppName;
        Folder = appState.Folder ?? Constants.ErrorAppName;

        // 2024-01-11 2dm - #RemoveIApp.Hidden for v17 - kill code ca. 2024-07 (Q3)
        //Hidden = AppStateInt.ConfigurationEntity?.Value<bool>(AppLoadConstants.FieldHidden) ?? false;
        //l.Done($"Name: {Name}, Folder: {Folder}, Hidden: {Hidden}");
    }
    #endregion

    [PrivateApi("kind of slipped into public till 16.09, but only on the object, never on the IApp, so probably never discovered")]
    public IAppState AppState => AppStateInt;

    protected internal IAppStateInternal AppStateInt => _appStateReader ??= Services.AppStates.GetReader(this);
    private IAppStateInternal _appStateReader;
}