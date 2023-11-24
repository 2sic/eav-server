using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data;

partial class Entity
{
    /// <inheritdoc />
    public IMetadataOf Metadata => _metadataOf.Get(() => _getMetadataOf(EntityGuid, GetBestTitle() ?? "entity with unknown title"));
    private readonly GetOnce<IMetadataOf> _metadataOf = new();
    private readonly Func<Guid, string, IMetadataOf> _getMetadataOf;

    /// <inheritdoc />
    public IEnumerable<Permission> Permissions => Metadata.Permissions;
}