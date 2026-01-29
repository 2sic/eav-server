using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Metadata.Sys;

partial class Metadata<T>
{
    /// <summary>
    /// All "normal" metadata entities - so it hides the system-entities
    /// like permissions. This is the default view of metadata given by an item
    /// </summary>
    [field: AllowNull, MaybeNull]
    private IList<IEntity> MetadataWithoutPermissions
    {
        get
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (field == null || UpStreamChanged())
                field = AllWithHidden
                    .Where(md => !Permission.IsPermission(md))
                    .ToListOpt();
            return field;
        }
        set;
    }

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public IEnumerable<IPermission> Permissions
    {
        get => field == null || UpStreamChanged()
            ? field = AllWithHidden.GetAll<Permission>()
                .ToImmutableOpt()
            : field;
        set;
    }

}