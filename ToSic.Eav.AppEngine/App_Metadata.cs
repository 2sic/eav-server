using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Apps
{
    public partial class App: IHasPermissions
    {
        #region Metadata

        public IMetadataOfItem Metadata
            => _metadata ?? (_metadata = new MetadataOf<int>(Constants.MetadataForApp, AppId,
                   _deferredLookupData));

        private IMetadataOfItem _metadata;
        private readonly IDeferredEntitiesList _deferredLookupData;

        public IEnumerable<IEntity> Permissions => Metadata.Permissions;

        #endregion

    }
}
