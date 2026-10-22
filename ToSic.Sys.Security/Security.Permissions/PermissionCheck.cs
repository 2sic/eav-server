using System.Diagnostics.CodeAnalysis;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Users;

namespace ToSic.Sys.Security.Permissions;

/// <summary>
/// Basic constructor, you must always call Init afterward
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class PermissionCheck(PermissionCheck.Dependencies services)
    : ServiceWithSetup<PermissionCheck.Options>("Sys.PrmChk", connect: [services]), IPermissionCheck
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Permissions"></param>
    /// <param name="TargetOwner">Information who the owner is for the item we're checking for - if any.</param>
    /// <param name="Env"></param>
    /// <param name="User"></param>
    public record Options(List<IPermission> Permissions, string? TargetOwner, IEnvironmentPermission? Env, IUser? User)
    {
        /// <summary>Information who the owner is for the item we're checking for - if any.</summary>
        public string TargetOwner { get; init; } = TargetOwner ?? "unknown-random239s;o";
    }

    #region MyServices

    public record Dependencies(ISysFeaturesService Features, IEnvironmentPermission EnvironmentPermission, IUser User)
        : DependenciesBase(connect: [Features, EnvironmentPermission]);

    #endregion

    #region Permission Targets and resulting list of metadata to control

    [field: AllowNull, MaybeNull]
    private IEnvironmentPermission EnvPermissions => field ??= MyOptions.Env ?? services.EnvironmentPermission;

    ///// <summary>
    ///// 
    ///// </summary>
    //private string? TargetItemOwner { get; set; }

    [field: AllowNull, MaybeNull]
    protected List<IPermission> PermissionList => field ??= MyOptions.Permissions ?? throw new NullReferenceException($"{nameof(PermissionList)} is null, it appears that you didn't call {nameof(Setup)}");

    public bool HasPermissions => PermissionList.Any();
    #endregion

    public static List<IPermission> CombinePermissions(IEnumerable<IHasPermissions?> hasPermissions, IEnumerable<IPermission?>? permissions)
    {
        List<IPermission> list =
        [
            ..hasPermissions.SelectMany(tp => tp?.Permissions ?? []),
            ..permissions?.OfType<IPermission>() ?? []
        ];
        return list;
    }

    //public void InitTargets(List<IPermission> permissions, string? targetOwner, IEnvironmentPermission? env, IUser? user) 
    //{
    //    var l = Log.Fn();
    //    //PermissionList = permissions;

    //    //TargetItemOwner = targetOwner ?? "unknown-random239s;o";
    //    //EnvPermissions = env!;
    //    //User = user;

    //    l.Done($"permissions: {PermissionList.Count}");
    //}

    public PermissionCheckInfo UserMay(List<Grants> grants)
    {
        var l = Log.Fn<PermissionCheckInfo>(Log.Try(() => $"[{string.Join(",", grants)}]"));

        var envPermissions = EnvPermissions.EnvironmentAllows(grants);
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
            var info = PermissionAllows(permission, [.. grants.Select(g => (char)g)]);
            if (info.Allowed)
                return l.ReturnAndLog(info);
        }

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
    private IUser User => field ??= MyOptions.User ?? services.User;
}