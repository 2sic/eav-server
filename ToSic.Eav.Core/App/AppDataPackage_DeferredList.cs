using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.App
{
	/// <inheritdoc cref="IDeferredEntitiesList" />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppDataPackage : IDeferredEntitiesList, IHasMetadataSource
    {

        IMetadataSource IHasMetadataSource.Metadata => Metadata;
    }
}