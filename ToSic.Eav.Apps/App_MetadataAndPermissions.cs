using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;
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
            var wrapLog = Log.Fn();

            var appState = AppState;
            Metadata = appState.Metadata;

            // Get the content-items describing various aspects of this app
            AppResources = appState.ResourcesInApp.MetadataItem;
            AppSettings = appState.SettingsInApp.MetadataItem;
            AppConfiguration = appState.SettingsInApp.AppConfiguration;
            // in some cases these things may be null, if the app was created not allowing side-effects
            // This can usually happen when new apps are being created
            Log.A($"HasResources: {AppResources != null}, HasSettings: {AppSettings != null}, HasConfiguration: {AppConfiguration != null}");

            // resolve some values for easier access
            Name = appState.Name ?? Constants.ErrorAppName;
            Folder = appState.Folder ?? Constants.ErrorAppName;

            Hidden = AppConfiguration?.Value<bool>(AppConstants.FieldHidden) ?? false;
            Log.A($"Name: {Name}, Folder: {Folder}, Hidden: {Hidden}");
            wrapLog.Done();
        }
        #endregion

        [PublicApi]
        public AppState AppState => _appState ?? (_appState = _dependencies.AppStates.Get(this));
        private AppState _appState;
    }
}
