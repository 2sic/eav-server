﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
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
                var logWrap = Log.Call();
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

                logWrap($"found {PermissionList.Count()} items");
                return _permissionList;
            }
        }

        private IEnumerable<Permission> _permissionList;

        private IEnumerable<Permission> _additionalMetadata;

        public bool HasPermissions => PermissionList.Any();

        #endregion


        #region constructors

        /// <summary>
        /// Basic constructor, you must always call Init afterwards
        /// </summary>
        protected PermissionCheckBase(string logName): base(logName) { }

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        protected PermissionCheckBase Init(
            ILog parentLog,
            IContentType targetType = null, // optional type to check
            IEntity targetItem = null,      // optional entity to check
            IEnumerable<Permission> permissions1 = null,
            IEnumerable<Permission> permissions2 = null
            ) 
        {
            Log.LinkTo(parentLog);
            var permList2 = permissions2 as IList<Permission> ?? permissions2?.ToList();

            var wrapLog = Log.Call($"type:{targetType?.StaticName}, " +
                                            $"itm:{targetItem?.EntityGuid} ({targetItem?.EntityId}), " +
                                            $"permList1: {permissions1?.Count()}, " +
                                            $"permList2: {permList2?.Count}");

            TargetType = targetType;
            TargetItem = targetItem;

            _additionalMetadata = permissions1 ?? new List<Permission>();
            if (permList2 != null)
                _additionalMetadata = _additionalMetadata.Concat(permList2);

            GrantedBecause = Conditions.Undefined;
            wrapLog("ready");
            return this;
        }

        #endregion

        public Conditions GrantedBecause
        {
            get;
            protected set;
        }

        public bool UserMay(List<Grants> grants)
        {
            var wrapLog = Log.Call(() => $"[{string.Join(",", grants)}]");
            GrantedBecause = Conditions.Undefined;
            var result = EnvironmentAllows(grants) || DoesPermissionsListAllow(grants);
            wrapLog($"{result} ({GrantedBecause})");
            return result;
        }


        /// <summary>
        /// Check if the permission-list would allow such an action
        /// </summary>
        /// <param name="grants">The desired action like c, r, u, d etc.</param>
        /// <returns></returns>
        private bool DoesPermissionsListAllow(List<Grants> grants)
        {
            var wrapLog = Log.Call(() => $"[{string.Join(", ", grants)}]", () => $"for {PermissionList.Count()} permission items");
            var result = PermissionList.Any(
                perm => DoesPermissionAllow(perm,
                    grants.Select(g => (char) g).ToArray()));
            wrapLog($"{result}");
            return result;
        }

        /// <summary>
        /// Check if a specific permission entity allows for the desired permission
        /// </summary>
        /// <param name="permissionEntity">The entity describing a permission</param>
        /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
        /// <returns></returns>
        private bool DoesPermissionAllow(Permission permissionEntity, char[] desiredActionCode)
        {
            var wrapLog = Log.Call($"{new string(desiredActionCode)}");
            // Check if it's a grant for the desired action - otherwise stop here
            var grant = permissionEntity.Grant;
            // If Grant doesn't contain desired action, stop here
            // otherwise check if it applies
            var result = grant.IndexOfAny(desiredActionCode) != -1 
                && VerifyConditionApplies(permissionEntity);
            wrapLog($"{result}");
            return result;
        }

        /// <summary>
        /// This should evaluate the grants and decide if the environment approves any of these grants.
        /// Note that in many cases the implementation will simply check if the environment provides edit permissions, but
        /// it can really check the grants required and compare each one with the environment.
        /// </summary>
        /// <param name="grants"></param>
        /// <returns></returns>
        protected abstract bool EnvironmentAllows(List<Grants> grants);


        /// <summary>
        /// Verify if a condition is a special code in the environment. 
        /// Example: a DNN code which asks for "registered users" or "view-users"
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        protected abstract bool VerifyConditionOfEnvironment(string condition);

        /// <summary>
        /// The current user, as provided by injection
        /// </summary>
        protected abstract IUser User { get; }
    }
}