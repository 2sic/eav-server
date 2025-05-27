using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Helpers;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data;

partial record Entity
{
    /// <inheritdoc />
    public IMetadataOf Metadata => _metadataOf.Get(() => PartsLazy.GetMetadataOfDelegate(EntityGuid, GetBestTitle() ?? "entity with unknown title"));
    private readonly GetOnce<IMetadataOf> _metadataOf = new();

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions => Metadata.Permissions;
}