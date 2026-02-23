using ToSic.Eav.Models;
using ToSic.Sys.Security.Permissions;


namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// Helper object for constants related to the permissions-type
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[ModelSpecs(ContentType = ContentTypeName)]
public record Permission : ModelFromEntityBasic, IPermission
{
    public const string ContentTypeName = "PermissionConfiguration";

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
    public static bool IsPermission(IEntity entity) => entity.Type.Is(ContentTypeName);
}