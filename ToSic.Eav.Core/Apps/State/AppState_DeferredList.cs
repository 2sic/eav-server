using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
	/// <inheritdoc cref="IEntitiesSource" />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppState : IEntitiesSource, IHasMetadataSource
    {

        IMetadataSource IHasMetadataSource.Metadata => Metadata;
    }
}