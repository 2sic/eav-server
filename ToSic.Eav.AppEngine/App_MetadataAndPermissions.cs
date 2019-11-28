﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
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
        public IMetadataOf Metadata
            => _metadata ?? (_metadata = new MetadataOf<int>(Constants.MetadataForApp, AppId,
                   AppDataPackage));

        private IMetadataOf _metadata;
        protected readonly IHasMetadataSource AppDataPackage;

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
            Log.Add("init app resources");

            // Get the content-items describing various aspects of this app
            AppResources = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppResources);
            AppSettings = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppSettings);
            AppConfiguration = Metadata.FirstOrDefault(md => md.Type.StaticName == AppConstants.TypeAppConfig);

            // resolve some values for easier access
            Name = AppConfiguration?.GetBestValue("DisplayName")?.ToString() ?? "Error";
            Folder = AppConfiguration?.GetBestValue("Folder")?.ToString() ?? "Error";
            if (bool.TryParse(AppConfiguration?.GetBestValue("Hidden")?.ToString(), out var hidden))
                Hidden = hidden;
        }
        #endregion
    }
}
