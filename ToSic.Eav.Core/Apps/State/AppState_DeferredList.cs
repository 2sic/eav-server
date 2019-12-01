using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
	public partial class AppState : IEntitiesSource, IHasMetadataSource
    {
        /// <inheritdoc />
        IMetadataSource IHasMetadataSource.Metadata => Metadata;
    }
}