using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security
{
    public partial class PermissionCheckBase
    {

        /// <summary>
        /// Check if the current user fits the reason for this grant
        /// </summary>
        /// <returns></returns>
        private bool VerifyConditionApplies(Permission permission)
        {
            var wrapLog = Log.Call<bool>();
            try
            {
                // check general permissions
                var condition = permission.Condition;
                var identity = permission.Identity;
                Log.A($"condition:{condition}, identity:{identity}");

                // check custom permission based on the user Guid or owner
                if (User.Guid != null)
                {
                    // check owner conditions (only possible on target entities, not content-types)
                    if (VerifyUserIsItemOwner(condition, TargetItem, User))
                        return wrapLog("is-owner: true", IsGrantedBecause(Conditions.Identity));

                    // check if an identity was provided
                    if (!string.IsNullOrWhiteSpace(identity))
                    {
                        Log.A($"Check if user is user or group - identity: {identity}");
                        if (VerifyUserIsThisUser(identity, User))
                            return wrapLog("is-this-user: true", IsGrantedBecause(Conditions.Owner));

                        if (VerifyUserIsInGroup(identity, User))
                            return wrapLog("is-in-specified-group: true", IsGrantedBecause(Conditions.Group));
                    }
                }

                // this checks if the condition is a environment condition
                // for example, if it's a DNN code for "user may view something"
                if (_environmentPermission.VerifyConditionOfEnvironment(condition))
                    return wrapLog("environment: true", IsGrantedBecause(Conditions.EnvironmentInstance));

                return wrapLog("no-match: false", false);
            }
            catch
            {
                // something happened, in this case we assume that this rule doesn't grant anything
                return wrapLog("error: false", false);
            }
        }

        private bool IsGrantedBecause(Conditions reason)
        {
            GrantedBecause = reason;
            return true;
        }

        /// <summary>
        /// Verify if the permission refers to this user
        /// Note that this only works if the feature is enabled
        /// </summary>
        private bool VerifyUserIsThisUser(string identity, IUser user)
        {
            if (!_features.Enabled(BuiltInFeatures.PermissionCheckUsers.Guid)) return false;
            return identity == user.Guid.ToString();
        }

        /// <summary>
        /// Check if the user is in any of the user groups provided
        /// Note that this only works if this feature is enabled
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool VerifyUserIsInGroup(string identity, IUser user)
        {
            if (!_features.Enabled(BuiltInFeatures.PermissionCheckGroups.Guid)) return false;

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
