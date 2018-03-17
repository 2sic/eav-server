using System.Collections.Generic;
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

        }

        #endregion

        public bool UserMay(PermissionGrant grant) => DoesPermissionsListAllow((char)grant);

        public bool UserMay(List<PermissionGrant> grants)
            => EnvironmentAllows(grants)
               || grants.Any(grant => DoesPermissionsListAllow((char) grant));

        /// <summary>
        /// This should 
        /// </summary>
        /// <param name="grants"></param>
        /// <returns></returns>
        protected abstract bool EnvironmentAllows(List<PermissionGrant> grants);


        /// <summary>
        /// Check if the permission-list would allow such an action
        /// </summary>
        /// <param name="desiredGrant">The desired action like c, r, u, d etc.</param>
        /// <returns></returns>
        protected bool DoesPermissionsListAllow(char desiredGrant) 
            => PermissionList.Any(perm => DoesPermissionAllow(perm, desiredGrant));

        /// <summary>
        /// Check if a specific permission entity allows for the desired permission
        /// </summary>
        /// <param name="permissionEntity">The entity describing a permission</param>
        /// <param name="desiredActionCode">A key like r (for read), u (for update) etc. which is the level you want to check</param>
        /// <returns></returns>
        protected bool DoesPermissionAllow(IEntity permissionEntity, char desiredActionCode)
        {

            // Check if it's a grant for the desired action - otherwise stop here
            var grnt = permissionEntity.GetBestValue(Constants.PermissionGrant).ToString();
            if (grnt.IndexOf(desiredActionCode) == -1) // Grant doesn't contain desired action, so stop here
                return false;

            // now we know that the rule would give the permission 
            // ...IF it applies to the current user
            // so let's check that now

            // Check if the current user fits the reason for this grant
            try
            {
                // check general permissions
                var condition = permissionEntity.GetBestValue(Constants.PermissionCondition).ToString();

                // check custom permission based on the user Guid or owner
                if (User.Guid != null)  // we have to have a valid user
                {
                    // check owner conditions (only possible on target entities, not content-types)
                    if (condition == Constants.PermissionKeyOwner
                        && TargetItem != null
                        && TargetItem.Owner == User.IdentityToken)
                        return true;

                    // rule just for this user
                    if (permissionEntity.GetBestValue(Constants.PermissionIdentity).ToString() == User.Guid.ToString())
                        return true;
                }

                return EnvironmentApprovesCondition(condition);
            }
            catch
            {
                // something happened, in this case we assume that this rule cannot described a "is allowed"
                return false;
            }
        }

        protected abstract bool EnvironmentApprovesCondition(string condition);

        /// <summary>
        /// The current user, as provided by injection
        /// </summary>
        protected abstract IUser User { get; }


    }
}