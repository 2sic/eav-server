using System.Collections.Generic;
using System.Linq;
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

            var appState = State.Cache.Get(this);
            Metadata = new MetadataOf<int>(Constants.MetadataForApp, AppId, appState);

            // Get the content-items describing various aspects of this app
            AppResources = Metadata.FirstOrDefault(md => md.Type.StaticName == AppLoadConstants.TypeAppResources);
            AppSettings = Metadata.FirstOrDefault(md => md.Type.StaticName == AppLoadConstants.TypeAppSettings);
            AppConfiguration = Metadata.FirstOrDefault(md => md.Type.StaticName == AppLoadConstants.TypeAppConfig);
            // in some cases these things may be null, if the app was created not allowing side-effects
            // This can usually happen when new apps are being created
            Log.Add($"HasResources: {AppResources != null}, HasSettings: {AppSettings != null}, HasConfiguration: {AppConfiguration != null}");

            // resolve some values for easier access
            Name = appState.Name ?? "Error";
            Folder = appState.Folder ?? "Error";
            if (bool.TryParse(AppConfiguration?.GetBestValue("Hidden")?.ToString(), out var hidden))
                Hidden = hidden;
            Log.Add($"Name: {Name}, Folder: {Folder}, Hidden: {Hidden}");
            wrapLog(null);
        }
        #endregion
    }
}
