using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security.Permissions;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    public partial class App: IHasPermissions
    {
        #region Metadata and Permission Accessors

        /// <summary>
        /// Metadata for this app (describing the app itself)
        /// </summary>
        public IMetadataOfItem Metadata
            => _metadata ?? (_metadata = new MetadataOf<int>(Constants.MetadataForApp, AppId,
                   AppDataPackage));

        private IMetadataOfItem _metadata;
        protected readonly IDeferredEntitiesList AppDataPackage;

        /// <summary>
        /// Permissions of this app
        /// </summary>
        public IEnumerable<IEntity> Permissions => Metadata.Permissions;

        #endregion

        #region Settings, Config, Metadata
        protected IEntity AppMetadata;
        protected IEntity AppSettings;
        protected IEntity AppResources;

        /// <summary>
        /// Assign all kinds of metadata / resources / settings (App-Mode only)
        /// </summary>
        protected void InitializeResourcesSettingsAndMetadata()
        {
            Log.Add("init app resources");

            // Get the content-items describing various aspects of this app
            AppResources = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppResources);
            AppSettings = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppSettings);
            AppMetadata = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppConfig);

            // resolve some values for easier access
            Name = AppMetadata?.GetBestValue("DisplayName")?.ToString() ?? "Error";
            Folder = AppMetadata?.GetBestValue("Folder")?.ToString() ?? "Error";
            if (bool.TryParse(AppMetadata?.GetBestValue("Hidden")?.ToString(), out var hidden))
                Hidden = hidden;
        }
        #endregion
    }
}
