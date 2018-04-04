using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        #region Metadata

        public IMetadataOfItem Metadata
            => _metadata ?? (_metadata = new MetadataOf<int>(Constants.MetadataForApp, AppId,
                   _deferredLookupData));

        private IMetadataOfItem _metadata;
        private readonly IDeferredEntitiesList _deferredLookupData;

        #endregion

    }
}
