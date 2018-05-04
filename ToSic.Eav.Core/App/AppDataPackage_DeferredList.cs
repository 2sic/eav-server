using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
	/// <inheritdoc cref="IDeferredEntitiesList" />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppDataPackage : IDeferredEntitiesList
    {

        IMetadataProvider IDeferredEntitiesList.Metadata => Metadata;
    }
}