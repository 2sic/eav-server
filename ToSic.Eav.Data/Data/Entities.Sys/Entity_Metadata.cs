using ToSic.Eav.Metadata;
using ToSic.Lib.Helpers;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data.Entities.Sys;

partial record Entity
{
    /// <inheritdoc />
    public IMetadataOf Metadata => _metadataOf.Get(() => PartsLazy.GetMetadataOfDelegate(EntityGuid, GetBestTitle() ?? "entity with unknown title"));
    private readonly GetOnce<IMetadataOf> _metadataOf = new();

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions => Metadata.Permissions;
}