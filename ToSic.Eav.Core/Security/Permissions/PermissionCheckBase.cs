using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;


namespace ToSic.Eav.Security
{
    public abstract partial class PermissionCheckBase : HasLog, IPermissionCheck
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

        public class Dependencies
        {
            public IFeaturesService Features { get; }
            public IEnvironmentPermission EnvironmentPermission { get; }

            public Dependencies(IFeaturesService features, IEnvironmentPermission environmentPermission)
            {
                Features = features;
                EnvironmentPermission = environmentPermission;
            }
        }

        /// <summary>
        /// Basic constructor, you must always call Init afterwards
        /// </summary>
        protected PermissionCheckBase(Dependencies dependencies, string logName): base(logName)
        {
            _features = dependencies.Features;
            _environmentPermission = dependencies.EnvironmentPermission;
        }
        private readonly IFeaturesService _features;
        private readonly IEnvironmentPermission _environmentPermission;

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        protected void Init(
            ILog parentLog,
            IContentType targetType = null, // optional type to check
            IEntity targetItem = null,      // optional entity to check
            IEnumerable<Permission> permissions1 = null,
            IEnumerable<Permission> permissions2 = null
            ) 
        {
            Log.LinkTo(parentLog);
            var permList2 = permissions2 as IList<Permission> ?? permissions2?.ToList();

            var wrapLog = Log.Fn($"type:{targetType?.NameId}, " +
                                            $"itm:{targetItem?.EntityGuid} ({targetItem?.EntityId}), " +
                                            $"permList1: {permissions1?.Count()}, " +
                                            $"permList2: {permList2?.Count}");

            TargetType = targetType;
            TargetItem = targetItem;

            _additionalMetadata = permissions1 ?? new List<Permission>();
            if (permList2 != null)
                _additionalMetadata = _additionalMetadata.Concat(permList2);

            GrantedBecause = Conditions.Undefined;
            wrapLog.Done("ready");
        }

        #endregion

        public Conditions GrantedBecause
        {
            get => _environmentPermission.GrantedBecause;
            protected set => _environmentPermission.GrantedBecause = value;
        }

        public bool UserMay(List<Grants> grants)
        {
            var wrapLog = Log.Fn<bool>(() => $"[{string.Join(",", grants)}]");
            GrantedBecause = Conditions.Undefined;
            var result = _environmentPermission.EnvironmentAllows(grants) || PermissionsAllow(grants);
            return wrapLog.Return(result, $"{result} ({GrantedBecause})");
        }


        /// <summary>
        /// Check if the permission-list would allow such an action
        /// </summary>
        /// <param name="grants">The desired action like c, r, u, d etc.</param>
        /// <returns></returns>
        public bool PermissionsAllow(IReadOnlyCollection<Grants> grants)
        {
            var wrapLog = Log.Fn<bool>(() => $"[{string.Join(", ", grants)}]", () => $"for {PermissionList.Count()} permission items");
            var result = PermissionList.Any(
                perm => PermissionAllows(perm,
                    grants.Select(g => (char) g).ToArray()));
            return wrapLog.ReturnAndLog(result);
        }

        /// <summary>
        /// Check if a specific permission entity allows for the desired permission
        /// </summary>
        /// <param name="permissionEntity">The entity describing a permission</param>
        /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
        /// <returns></returns>
        private bool PermissionAllows(Permission permissionEntity, char[] desiredActionCode)
        {
            var wrapLog = Log.Fn<bool>($"{new string(desiredActionCode)}");
            // Check if it's a grant for the desired action - otherwise stop here
            var grant = permissionEntity.Grant;
            // If Grant doesn't contain desired action, stop here
            // otherwise check if it applies
            var result = grant.IndexOfAny(desiredActionCode) != -1 
                && VerifyConditionApplies(permissionEntity);
            return wrapLog.ReturnAndLog(result);
        }
        

        /// <summary>
        /// The current user, as provided by injection
        /// </summary>
        protected abstract IUser User { get; }
    }
}