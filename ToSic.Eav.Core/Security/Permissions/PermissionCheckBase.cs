using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security;

/// <summary>
/// Basic constructor, you must always call Init afterward
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract partial class PermissionCheckBase(
    PermissionCheckBase.MyServices services,
    string logName,
    object[] connect = default)
    : ServiceBase<PermissionCheckBase.MyServices>(services, logName, connect: connect), IPermissionCheck
{
    #region MyServices

    public class MyServices(IEavFeaturesService features, IEnvironmentPermission environmentPermission)
        : MyServicesBase(connect: [features, environmentPermission])
    {
        public IEavFeaturesService Features { get; } = features;
        public IEnvironmentPermission EnvironmentPermission { get; } = environmentPermission;
    }

    #endregion

    #region Permission Targets and resulting list of metadata to control

    private IContentType TargetType { get; set; }

    private IEntity TargetItem { get; set; }

    protected List<Permission> PermissionList => field ??= BuildPermissionList();

    private List<Permission> BuildPermissionList()
    {
        var l = Log.Fn<List<Permission>>();

        List<Permission> list =
        [
            ..TargetItem?.Metadata.Permissions ?? [],
            ..TargetType?.Metadata.Permissions ?? [],
            .._additionalPermissions
        ];

        return l.Return(list, $"permissions: {list.Count}");
    }


    private List<Permission> _additionalPermissions;

    public bool HasPermissions => PermissionList.Any();

    #endregion




    /// <summary>
    /// Initialize this object so it can then give information regarding the permissions of an entity.
    /// Uses a GUID as identifier because that survives export/import. 
    /// </summary>
    protected void Init(
        IContentType targetType = default, // optional type to check
        IEntity targetItem = default,      // optional entity to check
        IEnumerable<Permission> permissions1 = default
    ) 
    {
        _additionalPermissions = permissions1?.ToList() ?? [];

        var l = Log.Fn($"type:{targetType?.NameId}, " +
                       $"itm:{targetItem?.EntityGuid} ({targetItem?.EntityId}), " +
                       $"permList1: {_additionalPermissions.Count}, ");

        TargetType = targetType;
        TargetItem = targetItem;

        GrantedBecause = Conditions.Undefined;
        l.Done();
    }


    public Conditions GrantedBecause
    {
        get => services.EnvironmentPermission.GrantedBecause;
        protected set => services.EnvironmentPermission.GrantedBecause = value;
    }

    public bool UserMay(List<Grants> grants)
    {
        var l = Log.Fn<bool>(Log.Try(() => $"[{string.Join(",", grants)}]"));
        GrantedBecause = Conditions.Undefined;
        var result = services.EnvironmentPermission.EnvironmentAllows(grants) || PermissionsAllow(grants);
        return l.Return(result, $"{result} ({GrantedBecause})");
    }


    /// <summary>
    /// Check if the permission-list would allow such an action
    /// </summary>
    /// <param name="grants">The desired action like c, r, u, d etc.</param>
    /// <returns></returns>
    public bool PermissionsAllow(IReadOnlyCollection<Grants> grants)
    {
        var l = Log.Fn<bool>(Log.Try(() => $"[{string.Join(", ", grants)}]"), Log.Try(() => $"for {PermissionList.Count()} permission items"));
        var result = PermissionList.Any(
            perm => PermissionAllows(perm,
                grants.Select(g => (char) g).ToArray()));
        return l.ReturnAndLog(result);
    }

    /// <summary>
    /// Check if a specific permission entity allows for the desired permission
    /// </summary>
    /// <param name="permissionEntity">The entity describing a permission</param>
    /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
    /// <returns></returns>
    private bool PermissionAllows(Permission permissionEntity, char[] desiredActionCode)
    {
        var l = Log.Fn<bool>($"{new string(desiredActionCode)}");
        // Check if it's a grant for the desired action - otherwise stop here
        var grant = permissionEntity.Grant;
        // If Grant doesn't contain desired action, stop here
        // otherwise check if it applies
        var result = grant.IndexOfAny(desiredActionCode) != -1 
                     && VerifyConditionApplies(permissionEntity);
        return l.ReturnAndLog(result);
    }
        

    /// <summary>
    /// The current user, as provided by injection
    /// </summary>
    protected abstract IUser User { get; }
}