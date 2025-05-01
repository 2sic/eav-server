using ToSic.Eav.Data;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security;

/// <summary>
/// Helper object for constants related to the permissions-type
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Permission(IEntity entity) : EntityBasedType(entity)
{
    public const string TypeName = "PermissionConfiguration";
    public const string FieldIdentity = "Identity";
    public const string FieldCondition = "Condition";
    public const string FieldGrant = "Grant";
    public const string FieldOwner = "Owner"; // todo: unclear - this probably should be a global field name

    /// <summary>
    /// The condition in this permission - like "this rule applies to admins"
    /// The condition is usually a text-code by the hosting CMS
    /// </summary>
    public string Condition => field ??= Entity.Get<string>(FieldCondition);

    /// <summary>
    /// The identity this rule should apply to 
    /// This is usually a user guid or group-id; exact specs vary based on the hosting CMS
    /// </summary>
    public string Identity => field ??= Entity.Get<string>(FieldIdentity);

    /// <summary>
    /// The rights granted by this permission, usually a series of characters like r=read, u=update, etc.
    /// </summary>
    public string Grant => field ??= Entity.Get<string>(FieldGrant);

    [PrivateApi]
    public static bool IsPermission(IEntity entity) => entity.Type.Is(TypeName);
}