﻿using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract partial class PermissionCheckBase : ServiceBase<PermissionCheckBase.MyServices>, IPermissionCheck
{

    #region Permission Targets and resulting list of metadata to control
    private IContentType TargetType { get; set; }

    private IEntity TargetItem { get; set; }

    protected IEnumerable<Permission> PermissionList
    {
        get
        {
            // already constructed, use that
            if (_permissionList != null) return _permissionList;
            var logWrap = Log.Fn<IEnumerable<Permission>>();
            var partsToConsider = new[]
            {
                TargetItem?.Metadata.Permissions,
                TargetType?.Metadata.Permissions,
                _additionalMetadata
            };
            // bundle all permission metadata items
            _permissionList = partsToConsider
                .Where(permList => permList != null)
                .Aggregate(_permissionList = new List<Permission>(), (current, permList)
                    => current.Concat(permList))
                .ToList();

            return logWrap.Return(_permissionList, $"found {PermissionList.Count()} items");
        }
    }

    private IEnumerable<Permission> _permissionList;

    private IEnumerable<Permission> _additionalMetadata;

    public bool HasPermissions => PermissionList.Any();

    #endregion


    #region constructors

    public class MyServices(IEavFeaturesService features, IEnvironmentPermission environmentPermission)
        : MyServicesBase(connect: [features, environmentPermission])
    {
        public IEavFeaturesService Features { get; } = features;
        public IEnvironmentPermission EnvironmentPermission { get; } = environmentPermission;
    }

    /// <summary>
    /// Basic constructor, you must always call Init afterwards
    /// </summary>
    protected PermissionCheckBase(MyServices services, string logName): base(services, logName)
    {
        _environmentPermission = services.EnvironmentPermission;
    }
    private readonly IEnvironmentPermission _environmentPermission;

    /// <summary>
    /// Initialize this object so it can then give information regarding the permissions of an entity.
    /// Uses a GUID as identifier because that survives export/import. 
    /// </summary>
    protected void Init(
        IContentType targetType = default, // optional type to check
        IEntity targetItem = default,      // optional entity to check
        IEnumerable<Permission> permissions1 = default,
        IEnumerable<Permission> permissions2 = default
    ) 
    {
        var permList2 = permissions2 as IList<Permission> ?? permissions2?.ToList();

        Log.Do($"type:{targetType?.NameId}, " +
               $"itm:{targetItem?.EntityGuid} ({targetItem?.EntityId}), " +
               $"permList1: {permissions1?.Count()}, " +
               $"permList2: {permList2?.Count}",
            action: () =>
            {

                TargetType = targetType;
                TargetItem = targetItem;

                _additionalMetadata = permissions1 ?? new List<Permission>();
                if (permList2 != null)
                    _additionalMetadata = _additionalMetadata.Concat(permList2);

                GrantedBecause = Conditions.Undefined;
            });
    }

    #endregion

    public Conditions GrantedBecause
    {
        get => _environmentPermission.GrantedBecause;
        protected set => _environmentPermission.GrantedBecause = value;
    }

    public bool UserMay(List<Grants> grants)
    {
        var l = Log.Fn<bool>(Log.Try(() => $"[{string.Join(",", grants)}]"));
        GrantedBecause = Conditions.Undefined;
        var result = _environmentPermission.EnvironmentAllows(grants) || PermissionsAllow(grants);
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