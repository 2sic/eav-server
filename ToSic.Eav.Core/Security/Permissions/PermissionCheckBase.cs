﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Security.Permissions
{
    public abstract class PermissionCheckBase : HasLog, IPermissionCheck
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
                    TargetItem?.Metadata,
                    TargetType?.Metadata,
                    _additionalMetadata
                };
                // bundle all permission metadata items
                _permissionList = partsToConsider
                    .Where(mdList => mdList != null)
                    .Aggregate(_permissionList = new List<IEntity>(), (current, mdList)
                        => current.Concat(mdList.Where(md
                            => md.Type.StaticName == Constants.PermissionTypeName)))
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
            IMetadataOfItem meta1 = null, // optional additional metadata, like of an app
            IMetadataOfItem meta2 = null)  // optional additional metadata, like of a zone
            : base("App.PermCk", parentLog, $"init for type:{targetType?.StaticName}, " +
                                            $"itm:{targetItem?.EntityGuid} ({targetItem?.EntityId}), " +
                                            $"meta1: {meta1?.Count()}, " +
                                            $"meta2: {meta2?.Count()}")
        {
            TargetType = targetType;
            TargetItem = targetItem;

            _additionalMetadata = meta1 ?? meta2;
            if (meta1 != null && meta2 != null)
                _additionalMetadata = meta1.Concat(meta2);

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
            GrantedBecause = ConditionType.Undefined;
            return EnvironmentAllows(grants)
                   || DoesPermissionsListAllow(grants);
        }

        /// <summary>
        /// This should 
        /// </summary>
        /// <param name="grants"></param>
        /// <returns></returns>
        protected abstract bool EnvironmentAllows(List<Grants> grants);


        /// <summary>
        /// Check if the permission-list would allow such an action
        /// </summary>
        /// <param name="grants">The desired action like c, r, u, d etc.</param>
        /// <returns></returns>
        protected bool DoesPermissionsListAllow(List<Grants> grants) 
            => PermissionList.Any(
                perm => DoesPermissionAllow(perm, 
                grants.Select(g => (char)g).ToArray()));

        /// <summary>
        /// Check if a specific permission entity allows for the desired permission
        /// </summary>
        /// <param name="permissionEntity">The entity describing a permission</param>
        /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
        /// <returns></returns>
        protected bool DoesPermissionAllow(IEntity permissionEntity, char[] desiredActionCode)
        {
            // Check if it's a grant for the desired action - otherwise stop here
            var grnt = permissionEntity.GetBestValue(Constants.PermissionGrant).ToString();
            // If Grant doesn't contain desired action, stop here
            // otherwise check if it applies
            return grnt.IndexOfAny(desiredActionCode) != -1 
                && DoesConditionApply(permissionEntity);
        }

        /// <summary>
        /// Check if the current user fits the reason for this grant
        /// </summary>
        /// <returns></returns>
        private bool DoesConditionApply(IEntity permissionEntity)
        {
            try
            {
                // check general permissions
                var condition = permissionEntity.GetBestValue(Constants.PermissionCondition).ToString();

                // check custom permission based on the user Guid or owner
                if (User.Guid != null) // we have to have a valid user
                {
                    // check owner conditions (only possible on target entities, not content-types)
                    if (ConditionItemOwner(condition, TargetItem, User))
                    {
                        GrantedBecause = ConditionType.Identity;
                        return true;
                    }

                    // rule just for this user
                    if (ConditionUserIdentity(permissionEntity, User))
                    {
                        GrantedBecause = ConditionType.Owner;
                        return true;
                    }
                }

                if (DoesConditionApplyInEnvironment(condition))
                {
                    GrantedBecause = ConditionType.EnvironmentInstance;
                    return true;
                }

                return false;
            }
            catch
            {
                // something happened, in this case we assume that this rule cannot described a "is allowed"
                return false;
            }
        }

        /// <summary>
        /// Verify if the permission referrs to this user
        /// </summary>
        private static bool ConditionUserIdentity(IEntity permissionEntity, IUser user)
            => permissionEntity.GetBestValue(Constants.PermissionIdentity).ToString()
               == user.Guid.ToString();

        /// <summary>
        /// Verify that the permission is for owners, and the user is the item owner
        /// </summary>
        private static bool ConditionItemOwner(string condition, IEntity item, IUser user)
            => condition == Constants.PermissionKeyOwner
               && item?.Owner == user.IdentityToken;

        protected abstract bool DoesConditionApplyInEnvironment(string condition);

        /// <summary>
        /// The current user, as provided by injection
        /// </summary>
        protected abstract IUser User { get; }


    }
}