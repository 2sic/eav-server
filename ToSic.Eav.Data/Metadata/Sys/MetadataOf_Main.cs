﻿using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Metadata.Sys;

partial class MetadataOf<T>
{


    /// <summary>
    /// All "normal" metadata entities - so it hides the system-entities
    /// like permissions. This is the default view of metadata given by an item
    /// </summary>
    private IList<IEntity> MetadataWithoutPermissions
    {
        get
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (_metadataWithoutPermissions == null || UpStreamChanged())
                _metadataWithoutPermissions = AllWithHidden
                    .Where(md => !Permission.IsPermission(md))
                    .ToListOpt();
            return _metadataWithoutPermissions;
        }
    }
    private IList<IEntity> _metadataWithoutPermissions;

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions
    {
        get
        {
            if (_permissions == null || UpStreamChanged())
                _permissions = AllWithHidden
                    .Where(Permission.IsPermission)
                    .Select(IPermission (e) => new Permission(e))
                    .ToImmutableOpt();
            return _permissions;
        }
    }
    private IImmutableList<IPermission> _permissions;



}