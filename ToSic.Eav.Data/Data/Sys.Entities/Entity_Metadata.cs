using ToSic.Eav.Metadata;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data.Sys.Entities;

partial record Entity
{
    /// <inheritdoc />
    public IMetadata Metadata => field
        ??= PartsLazy.GetMetadataOfDelegate(EntityGuid, GetBestTitle() ?? "entity with unknown title");

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions => Metadata.Permissions;
}