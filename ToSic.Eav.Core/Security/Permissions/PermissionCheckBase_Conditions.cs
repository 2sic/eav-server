using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Security.Permissions
{
    public partial class PermissionCheckBase
    {

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

                    // check if an identity was provided
                    var identity = permissionEntity.GetBestValue(Constants.PermissionIdentity).ToString();
                    if (!string.IsNullOrWhiteSpace(identity))
                    {
                        // rule just for this user
                        if (ConditionUserIdentity(identity, User))
                        {
                            GrantedBecause = ConditionType.Owner;
                            return true;
                        }
                        if (ConditionUserGroup(identity, User))
                        {
                            GrantedBecause = ConditionType.Group;
                            return true;
                        }
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
        private static bool ConditionUserIdentity(string identity, IUser user)
        {
            if (!Features.Enabled(FeatureIds.PermissionCheckUserId)) return false;
            return identity == user.Guid.ToString();
        }

        private static bool ConditionUserGroup(string identity, IUser user)
        {
            if (!Features.Enabled(FeatureIds.PermissionCheckGroups)) return false;

            if (string.IsNullOrWhiteSpace(identity)) return false;

            var groupIds = identity.Split(',').Select(g => g.Trim())
                .Where(g => !string.IsNullOrWhiteSpace(g)).Select(
                    g => int.TryParse(g, out var gid) ? gid : 0)
                .Where(gid => gid > 0)
                .ToList();

            return user.Roles.Any(r => groupIds.Contains(r));
        }

        /// <summary>
        /// Verify that the permission is for owners, and the user is the item owner
        /// </summary>
        private static bool ConditionItemOwner(string condition, IEntity item, IUser user)
            => condition == Constants.PermissionKeyOwner
               && item?.Owner == user.IdentityToken;

    }
}
