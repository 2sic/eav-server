﻿using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Security;

namespace ToSic.Eav.Metadata;

partial class MetadataOf<T>
{


    /// <summary>
    /// All "normal" metadata entities - so it hides the system-entities
    /// like permissions. This is the default view of metadata given by an item
    /// </summary>
    private List<IEntity> MetadataWithoutPermissions
    {
        get
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (_metadataWithoutPermissions == null || UpStreamChanged())
                _metadataWithoutPermissions = AllWithHidden
                    .Where(md => !Permission.IsPermission(md))
                    .ToList();
            return _metadataWithoutPermissions;
        }
    }
    private List<IEntity> _metadataWithoutPermissions;

    /// <inheritdoc />
    public IEnumerable<Permission> Permissions
    {
        get
        {
            if (_permissions == null || UpStreamChanged())
                _permissions = AllWithHidden
                    .Where(Permission.IsPermission)
                    .Select(e => new Permission(e))
                    .ToImmutableList();
            return _permissions;
        }
    }
    private ImmutableList<Permission> _permissions;



}