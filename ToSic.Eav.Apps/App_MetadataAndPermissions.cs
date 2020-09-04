using System.Collections.Generic;
using System.Linq;
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
        public IMetadataOf Metadata
            => _metadata ?? (_metadata = new MetadataOf<int>(Constants.MetadataForApp, AppId,
                   AppState));

        private IMetadataOf _metadata;

        [PrivateApi]
        protected IHasMetadataSource AppState;

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

            // Get the content-items describing various aspects of this app
            AppResources = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppResources);
            AppSettings = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppSettings);
            AppConfiguration = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppConfig);
            Log.Add($"HasResources: {AppResources != null}, HasSettings: {AppSettings != null}, HasConfiguration: {AppConfiguration != null}");

            // resolve some values for easier access
            Name = AppConfiguration?.GetBestValue("DisplayName")?.ToString() ?? "Error";
            Folder = AppConfiguration?.GetBestValue("Folder")?.ToString() ?? "Error";
            if (bool.TryParse(AppConfiguration?.GetBestValue("Hidden")?.ToString(), out var hidden))
                Hidden = hidden;
            Log.Add($"Name: {Name}, Folder: {Folder}, Hidden: {Hidden}");
            wrapLog(null);
        }
        #endregion
    }
}
