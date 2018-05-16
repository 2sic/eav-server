using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Security.Permissions
{
    public abstract partial class PermissionCheckBase : HasLog, IPermissionCheck
    {

        #region Permission Targets and resulting list of metadata to control
        private IContentType TargetType { get; }

        private IEntity TargetItem { get; }

        protected IEnumerable<IEntity> PermissionList
        {
            get
            {
                // already constructed, use that
                if (_permissionList != null) return _permissionList;

                var partsToConsider = new[]
                {
                    TargetItem?.Metadata.Permissions,
                    TargetType?.Metadata.Permissions,
                    _additionalMetadata
                };
                // bundle all permission metadata items
                _permissionList = partsToConsider
                    .Where(permList => permList != null)
                    .Aggregate(_permissionList = new List<IEntity>(), (current, permList)
                        => current.Concat(permList))
                    .ToList();
                
                return _permissionList;
            }
        }

        private IEnumerable<IEntity> _permissionList;

        private readonly IEnumerable<IEntity> _additionalMetadata;

        public bool HasPermissions => PermissionList.Any();

        #endregion


        #region constructors

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        protected PermissionCheckBase(
            Log parentLog,
            IContentType targetType = null, // optional type to check
            IEntity targetItem = null,      // optional entity to check
            IEnumerable<IEntity> permissions1 = null,
            IEnumerable<IEntity> permissions2 = null
            ) 
            : base("App.PermCk", parentLog, $"init for type:{targetType?.StaticName}, " +
                                            $"itm:{targetItem?.EntityGuid} ({targetItem?.EntityId}), " +
                                            $"meta1: {permissions1?.Count()}, " +
                                            $"meta2: {permissions2?.Count()}")
        {
            TargetType = targetType;
            TargetItem = targetItem;

            _additionalMetadata = permissions1 ?? new List<IEntity>();
            if (permissions2 != null)
                _additionalMetadata = _additionalMetadata.Concat(permissions2);

            GrantedBecause = ConditionType.Undefined;

        }

        #endregion

        public bool UserMay(Grants grant) 
            => UserMay(new List<Grants> {grant});

        public ConditionType GrantedBecause
        {
            get;
            protected set;
        }

        public bool UserMay(List<Grants> grants)
        {
            Log.Add("user may...");
            GrantedBecause = ConditionType.Undefined;
            return EnvironmentAllows(grants)
                   || DoesPermissionsListAllow(grants);
        }


        /// <summary>
        /// Check if the permission-list would allow such an action
        /// </summary>
        /// <param name="grants">The desired action like c, r, u, d etc.</param>
        /// <returns></returns>
        private bool DoesPermissionsListAllow(List<Grants> grants) 
            => PermissionList.Any(
                perm => DoesPermissionAllow(perm, 
                grants.Select(g => (char)g).ToArray()));

        /// <summary>
        /// Check if a specific permission entity allows for the desired permission
        /// </summary>
        /// <param name="permissionEntity">The entity describing a permission</param>
        /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
        /// <returns></returns>
        private bool DoesPermissionAllow(IEntity permissionEntity, char[] desiredActionCode)
        {
            Log.Add($"does perm list allow {desiredActionCode}");
            // Check if it's a grant for the desired action - otherwise stop here
            var grnt = permissionEntity.GetBestValue(Constants.PermissionGrant).ToString();
            // If Grant doesn't contain desired action, stop here
            // otherwise check if it applies
            return grnt.IndexOfAny(desiredActionCode) != -1 
                && DoesConditionApply(permissionEntity);
        }

        /// <summary>
        /// This should 
        /// </summary>
        /// <param name="grants"></param>
        /// <returns></returns>
        protected abstract bool EnvironmentAllows(List<Grants> grants);


        protected abstract bool DoesConditionApplyInEnvironment(string condition);

        /// <summary>
        /// The current user, as provided by injection
        /// </summary>
        protected abstract IUser User { get; }


    }
}