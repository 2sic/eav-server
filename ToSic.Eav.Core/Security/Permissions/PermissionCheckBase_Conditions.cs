using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security.Permissions
{
    public partial class PermissionCheckBase
    {

        /// <summary>
        /// Check if the current user fits the reason for this grant
        /// </summary>
        /// <returns></returns>
        private bool VerifyConditionApplies(Permission permission)
        {
            //var wrapLog = Log.Call<bool>("VerifyConditionApplies");
            try
            {
                // check general permissions
                var condition = permission.Condition;// permissionEntity.GetBestValue<string>(Permission.FieldCondition);
                var identity = permission.Identity;// permissionEntity.GetBestValue<string>(Permission.FieldIdentity);
                Log.Add($"condition:{condition}, identity:{identity}");

                // check custom permission based on the user Guid or owner
                if (User.Guid != null)
                {
                    // check owner conditions (only possible on target entities, not content-types)
                    if (VerifyUserIsItemOwner(condition, TargetItem, User))
                        return IsGrantedBecause(ConditionType.Identity);

                    // check if an identity was provided
                    if (!string.IsNullOrWhiteSpace(identity))
                    {
                        if (VerifyUserIsThisUser(identity, User))
                            return IsGrantedBecause(ConditionType.Owner);

                        if (VerifyUserIsInGroup(identity, User))
                            return IsGrantedBecause(ConditionType.Group);
                    }
                }

                // this checks if the condition is a environment condition
                // for example, if it's a DNN code for "user may view something"
                if (VerifyConditionOfEnvironment(condition))
                    return IsGrantedBecause(ConditionType.EnvironmentInstance);

                return false;
            }
            catch
            {
                // something happened, in this case we assume that this rule doesn't grant anything
                return false;
            }
        }

        private bool IsGrantedBecause(ConditionType reason)
        {
            GrantedBecause = reason;
            return true;
        }

        /// <summary>
        /// Verify if the permission refers to this user
        /// Note that this only works if the feature is enabled
        /// </summary>
        private static bool VerifyUserIsThisUser(string identity, IUser user)
        {
            if (!Features.Enabled(FeatureIds.PermissionCheckUserId)) return false;
            return identity == user.Guid.ToString();
        }

        /// <summary>
        /// Check if the user is in any of the user groups provided
        /// Note that this only works if this feature is enabled
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool VerifyUserIsInGroup(string identity, IUser user)
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
        private static bool VerifyUserIsItemOwner(string condition, IEntity item, IUser user)
            => condition == Permission.FieldOwner && item?.Owner == user.IdentityToken;
    }
}
