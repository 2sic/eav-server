using ToSic.Lib.Services;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Users;

namespace ToSic.Sys.Security.Permissions;

/// <summary>
/// Basic constructor, you must always call Init afterward
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract partial class PermissionCheckBase(PermissionCheckBase.MyServices services, string logName, object[] connect = default)
    : ServiceBase<PermissionCheckBase.MyServices>(services, logName, connect: connect), IPermissionCheck
{
    // ReSharper disable once InconsistentNaming
    private readonly MyServices services = services;

    #region MyServices

    public class MyServices(IEavFeaturesService features, IEnvironmentPermission environmentPermission)
        : MyServicesBase(connect: [features, environmentPermission])
    {
        public IEavFeaturesService Features { get; } = features;
        public IEnvironmentPermission EnvironmentPermission { get; } = environmentPermission;
    }

    #endregion

    #region Permission Targets and resulting list of metadata to control

    //private IContentType TargetType { get; set; }
    private IHasPermissions TargetTypePermissionsOrNull { get; set; }

    //private IEntity TargetItem { get; set; }
    private IHasPermissions TargetItemPermissionsOrNull { get; set; }
    private string TargetItemOwner { get; set; }

    protected List<IPermission> PermissionList => field ??= BuildPermissionList();

    private List<IPermission> BuildPermissionList()
    {
        var l = Log.Fn<List<IPermission>>();

        List<IPermission> list =
        [
            ..TargetTypePermissionsOrNull?.Permissions ?? [],
            ..TargetItemPermissionsOrNull?.Permissions ?? [],
            .._additionalPermissions
        ];

        return l.Return(list, $"permissions: {list.Count}");
    }


    private List<IPermission> _additionalPermissions;

    public bool HasPermissions => PermissionList.Any();

    #endregion


    ///// <summary>
    ///// Initialize this object so it can then give information regarding the permissions of an entity.
    ///// Uses a GUID as identifier because that survives export/import. 
    ///// </summary>
    ///// <param name="targetType">optional type to check</param>
    ///// <param name="targetItem">optional entity to check</param>
    ///// <param name="permissions"></param>
    ///// 
    //protected void Init(IContentType targetType = default, IEntity targetItem = default, IEnumerable<IPermission> permissions = default) 
    //{
    //    _additionalPermissions = permissions?.ToList() ?? [];

    //    var l = Log.Fn($"type:{targetType?.NameId}, " +
    //                   $"itm:{targetItem?.EntityGuid} ({targetItem?.EntityId}), " +
    //                   $"permList1: {_additionalPermissions.Count}, ");

    //    TargetTypePermissionsOrNull = targetType?.Metadata;
    //    TargetItemPermissionsOrNull = targetItem?.Metadata;
    //    TargetItemOwner = targetItem?.Owner ?? "unknown-random239s;o";

    //    GrantedBecause = Conditions.Undefined;
    //    l.Done();
    //}
    protected void InitTargets(IHasPermissions typePermissions = default, IHasPermissions itemPermissions = default, string targetOwner = default, IEnumerable<IPermission> permissions = default) 
    {
        var l = Log.Fn();

        _additionalPermissions = permissions?.ToList() ?? [];
        
        TargetTypePermissionsOrNull = typePermissions;
        TargetItemPermissionsOrNull = itemPermissions;
        TargetItemOwner = targetOwner ?? "unknown-random239s;o";

        l.Done($"permissions: {_additionalPermissions.Count}");
    }

    public PermissionCheckInfo UserMay(List<Grants> grants)
    {
        var l = Log.Fn<PermissionCheckInfo>(Log.Try(() => $"[{string.Join(",", grants)}]"));

        var envPermissions = services.EnvironmentPermission.EnvironmentAllows(grants);
        if (envPermissions.Allowed)
            return l.ReturnAndLog(envPermissions);

        var result = PermissionsAllow(grants);
        return l.Return(result, $"{result}");
    }


    /// <summary>
    /// Check if the permission-list would allow such an action
    /// </summary>
    /// <param name="grants">The desired action like c, r, u, d etc.</param>
    /// <returns></returns>
    public PermissionCheckInfo PermissionsAllow(IReadOnlyCollection<Grants> grants)
    {
        var l = Log.Fn<PermissionCheckInfo>(Log.Try(() => $"[{string.Join(", ", grants)}]"), Log.Try(() => $"for {PermissionList.Count()} permission items"));

        // Loop through permissions, and check if any of them allows the desired action
        foreach (var permission in PermissionList)
        {
            var info = PermissionAllows(permission, grants.Select(g => (char)g).ToArray());
            if (info.Allowed)
                return l.ReturnAndLog(info);
        }

        //var result = PermissionList.FirstOrDefault(
        //    perm => PermissionAllows(perm, grants.Select(g => (char) g).ToArray()));
        return l.ReturnAndLog(new(false, Conditions.Undefined));
    }

    /// <summary>
    /// Check if a specific permission entity allows for the desired permission
    /// </summary>
    /// <param name="permissionEntity">The entity describing a permission</param>
    /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
    /// <returns></returns>
    private PermissionCheckInfo PermissionAllows(IPermission permissionEntity, char[] desiredActionCode)
    {
        var l = Log.Fn<PermissionCheckInfo>($"{new string(desiredActionCode)}");
        // Check if it's a grant for the desired action - otherwise stop here
        var grant = permissionEntity.Grant;
        // If Grant doesn't contain desired action, stop here
        if (grant.IndexOfAny(desiredActionCode) == -1)
            return l.Return(new(false, Conditions.Undefined), "no grant-match");

        // otherwise check if it applies
        var result = VerifyConditionApplies(permissionEntity);
        return l.ReturnAndLog(result);
    }
        

    /// <summary>
    /// The current user, as provided by injection
    /// </summary>
    protected abstract IUser User { get; }
}