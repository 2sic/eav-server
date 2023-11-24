using ToSic.Eav.Data.Source;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
	partial class AppState : IEntitiesSource, IHasMetadataSource
    {
        /// <inheritdoc />
        IMetadataSource IHasMetadataSource.MetadataSource => _metadataManager;
    }
}