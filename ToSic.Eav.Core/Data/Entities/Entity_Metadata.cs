using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data;

partial record Entity
{
    /// <inheritdoc />
    public IMetadataOf Metadata => _metadataOf.Get(() => PartsBuilder.GetMetadataOfDelegate(EntityGuid, GetBestTitle() ?? "entity with unknown title"));
    private readonly GetOnce<IMetadataOf> _metadataOf = new();

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions => Metadata.Permissions;
}