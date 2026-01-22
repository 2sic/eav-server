using ToSic.Eav.Data.Sys.Entities;
using ToSic.Sys.Security.Permissions;


namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// Helper object for constants related to the permissions-type
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record Permission : RecordOfEntityBase, IPermission
{
    public Permission(IEntity entity) : base(entity) { }

    public const string TypeName = "PermissionConfiguration";
    public const string FieldOwner = "Owner";
    /// <inheritdoc />

    [field: AllowNull, MaybeNull]
    public string Condition => field ??= GetThis("");

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public string Identity => field ??= GetThis<string>("");

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public string Grant => field ??= GetThis<string>("");

    /// <inheritdoc />
    public string Owner => Entity.Owner;

    [PrivateApi]
    public static bool IsPermission(IEntity entity) => entity.Type.Is(TypeName);
}