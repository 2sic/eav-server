using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Security.Permissions
{
    public abstract class PermissionController : HasLog
    {
        protected IContentType TargetType { get; }

        public IEntity TargetItem { get; set; }

        public IEnumerable<IEntity> PermissionList
        {
            get
            {
                if (_permissionList != null) return _permissionList;

                // start with the type OR item metadata-list
                _permissionList = (TargetType?.Metadata ?? TargetItem?.Metadata)?
                    .Where(md => md.Type.StaticName == Constants.PermissionTypeName)
                    .ToList()
                    ??  new List<IEntity>(); // use empty if none found

                // see if we should attach anything additional
                if (_additionalMetadata != null)
                    _permissionList = _additionalMetadata.Where(md => md.Type.StaticName == Constants.PermissionTypeName)
                        .ToList().Concat(_permissionList);

                return _permissionList;
            }
        }

        private IEnumerable<IEntity> _permissionList;

        private readonly IEnumerable<IEntity> _additionalMetadata;

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="parentLog"></param>
        protected PermissionController(
            IEntity targetItem,
            Log parentLog)
            : base("App.PermCk", parentLog, $"init for itm:{targetItem?.EntityGuid} ({targetItem?.EntityId})")
        {
            TargetItem = targetItem;
            TargetType = null; // important that it doesn't exist, otherwise the security check will use that instead of the item
        }

        protected PermissionController(
            IContentType targetType, 
            IEntity targetItem, 
            Log parentLog, 
            IMetadataOfItem addMeta1 = null, // optional additional metadata, like of an app
            IMetadataOfItem addMeta2 = null)  // optional additional metadata, like of a zone
            : this(addMeta1, addMeta2, parentLog, $"init for type:{targetType?.StaticName}, itm:{targetItem?.EntityGuid} ({targetItem?.EntityId})")
        {
            TargetType = targetType;
            TargetItem = targetItem;
        }

        protected PermissionController(
            IMetadataOfItem meta1 = null, // optional additional metadata, like of an app
            IMetadataOfItem meta2 = null,  // optional additional metadata, like of a zone
            Log parentLog = null,
            string message = null
            ) : base("App.PermCk", parentLog, $"{message} - init for meta:{meta1 != null}, 2:{meta2 != null}")
        {
            _additionalMetadata = meta1 ?? meta2;
            if (meta1 != null && meta2 != null)
                _additionalMetadata = meta1.Concat(meta2);
        }


        /// <summary>
        /// Check if a user may do something based on the permissions in the background. 
        /// </summary>
        /// <param name="actionCode">Short-code for r=read, u=update etc.</param>
        /// <returns></returns>
        public bool UserMay(char actionCode) => DoesPermissionsListAllow(actionCode);

        public bool UserMay(PermissionGrant action) => DoesPermissionsListAllow((char)action);

        public bool UserMay(List<PermissionGrant> actions) => actions.Any(action => DoesPermissionsListAllow((char)action));

        /// <summary>
        /// Check if the permission-list would allow such an action
        /// </summary>
        /// <param name="desiredActionCode">The desired action like c, r, u, d etc.</param>
        /// <returns></returns>
        protected bool DoesPermissionsListAllow(char desiredActionCode) 
            => PermissionList.Any(perm => DoesPermissionAllow(perm, desiredActionCode));

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

                if (EnvironmentGivesPermission(condition))
                    return true;
            }
            catch
            {
                // something happened, in this case we assume that this rule cannot described a "is allowed"
                return false;
            }

            // If the code gets here, we apparently don't know what the rule is about - return false
            return false;
        }

        protected abstract bool EnvironmentGivesPermission(string condition);

        /// <summary>
        /// The current user, as provided by injection
        /// </summary>
        protected abstract IUser User { get; }

    }
}