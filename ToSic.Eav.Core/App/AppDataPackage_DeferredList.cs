using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
	/// <inheritdoc cref="IDeferredEntitiesList" />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppDataPackage : IDeferredEntitiesList
    {
        //public AppDataPackageDeferredList BetaDeferred { get; }


        //public IEnumerable<IEntity> Entities => List;

        IMetadataProvider IDeferredEntitiesList.Metadata => Metadata;
    }
}