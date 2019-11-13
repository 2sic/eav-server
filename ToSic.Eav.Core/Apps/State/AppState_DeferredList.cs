using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.App
{
	/// <inheritdoc cref="IEntitiesSource" />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppDataPackage : IEntitiesSource, IHasMetadataSource
    {

        IMetadataSource IHasMetadataSource.Metadata => Metadata;
    }
}