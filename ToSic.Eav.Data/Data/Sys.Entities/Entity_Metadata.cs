using ToSic.Eav.Metadata;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data.Sys.Entities;

partial record Entity
{
    /// <inheritdoc />
    public IMetadata Metadata => _metadataOf.Get(() => PartsLazy.GetMetadataOfDelegate(EntityGuid, GetBestTitle() ?? "entity with unknown title"))!;
    private readonly GetOnce<IMetadata> _metadataOf = new();

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions => Metadata.Permissions;
}