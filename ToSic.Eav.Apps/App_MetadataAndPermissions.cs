using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    public partial class App: IHasPermissions
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
            var wrapLog = Log.Call();

            var appState = AppState;// State.Get(this);
            Metadata = appState.AppMetadata;

            // Get the content-items describing various aspects of this app
            AppResources = appState.SettingsInApp.StackCache[AppThingsToStack.Resources].AppItem;// .AppResources;
            AppSettings = appState.SettingsInApp.StackCache[AppThingsToStack.Settings].AppItem;// .AppSettings;
            AppConfiguration = appState.SettingsInApp.AppConfiguration;
            // in some cases these things may be null, if the app was created not allowing side-effects
            // This can usually happen when new apps are being created
            Log.Add($"HasResources: {AppResources != null}, HasSettings: {AppSettings != null}, HasConfiguration: {AppConfiguration != null}");

            // resolve some values for easier access
            Name = appState.Name ?? "Error";
            Folder = appState.Folder ?? "Error";
            Hidden = AppConfiguration?.Value<bool>(AppConstants.FieldHidden) ?? false;
            Log.Add($"Name: {Name}, Folder: {Folder}, Hidden: {Hidden}");
            wrapLog(null);
        }
        #endregion

        [PrivateApi]
        public AppState AppState => _appState ?? (_appState = State.Get(this));
        private AppState _appState;
    }
}
