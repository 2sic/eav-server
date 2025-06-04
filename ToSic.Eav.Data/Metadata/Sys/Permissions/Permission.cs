using ToSic.Eav.Data.EntityBased.Sys;
using ToSic.Sys.Security.Permissions;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// Helper object for constants related to the permissions-type
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class Permission(IEntity entity) : EntityBasedType(entity), IPermission
{
    public const string TypeName = "PermissionConfiguration";

    /// <inheritdoc />
    public string Condition => field ??= GetThis<string>(null);

    /// <inheritdoc />
    public string Identity => field ??= GetThis<string>(null);

    /// <inheritdoc />
    public string Grant => field ??= GetThis<string>(null);

    /// <inheritdoc />
    public string Owner => Entity.Owner;

    [PrivateApi]
    public static bool IsPermission(IEntity entity) => entity.Type.Is(TypeName);
}