using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Apps
{
    public partial class App: IHasPermissions
    {
        #region Metadata

        /// <summary>
        /// Metadata for this app (describing the app itself)
        /// </summary>
        public IMetadataOfItem Metadata
            => _metadata ?? (_metadata = new MetadataOf<int>(Constants.MetadataForApp, AppId,
                   _deferredLookupData));

        private IMetadataOfItem _metadata;
        private readonly IDeferredEntitiesList _deferredLookupData;

        /// <summary>
        /// Permissions of this app
        /// </summary>
        public IEnumerable<IEntity> Permissions => Metadata.Permissions;

        #endregion

    }
}
