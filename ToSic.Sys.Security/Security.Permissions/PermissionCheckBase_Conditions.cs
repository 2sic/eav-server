using ToSic.Sys.Users;
using ToSic.Sys.Utils;

namespace ToSic.Sys.Security.Permissions;

partial class PermissionCheckBase
{
    // TODO: THIS IS CURRENTLY A MANUAL COPY OF THE VALUE
    // This should not be the final state
    private static Guid PermissionCheckUsersGuid = new("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775");

    /// <summary>
    /// Check if the current user fits the reason for this grant
    /// </summary>
    /// <returns></returns>
    private PermissionCheckInfo VerifyConditionApplies(IPermission permission)
    {
        var l = Log.Fn<PermissionCheckInfo>();
        try
        {
            // check general permissions
            var condition = permission.Condition;
            var identity = permission.Identity;
            l.A($"condition:{condition}, identity:{identity}");

            // check custom permission based on the user Guid or owner
            if (User.Guid != default)
            {
                // check owner conditions (only possible on target entities, not content-types)
                if (VerifyUserIsItemOwner(condition, TargetItemOwner, User))
                    return l.Return(new(true, Conditions.Identity), "is-owner: true");

                // check if an identity was provided
                if (!string.IsNullOrWhiteSpace(identity))
                {
                    l.A($"Check if user is user or group - identity: {identity}");
                    if (VerifyUserIsThisUser(identity, User))
                        return l.Return(new(true, Conditions.Owner), "is-this-user: true");

                    if (VerifyUserIsInGroup(identity, User))
                        return l.Return(new(true, Conditions.Group), "is-in-specified-group: true");
                }
            }

            // this checks if the condition is a environment condition
            // for example, if it's a DNN code for "user may view something"
            if (services.EnvironmentPermission.VerifyConditionOfEnvironment(condition))
                return l.Return(new(true, Conditions.EnvironmentInstance), "environment: true");

            return l.Return(new (false, Conditions.Undefined), "no-match: false");
        }
        catch
        {
            // something happened, in this case we assume that this rule doesn't grant anything
            return l.Return(new(false, Conditions.Undefined), "error: false");
        }
    }

    /// <summary>
    /// Verify if the permission refers to this user
    /// Note that this only works if the feature is enabled
    /// </summary>
    private bool VerifyUserIsThisUser(string identity, IUser user)
    {
        if (!Services.Features.IsEnabled(PermissionCheckUsersGuid))
            return false;
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
        if (!Services.Features.IsEnabled(PermissionCheckUsersGuid))
            return false;

        if (string.IsNullOrWhiteSpace(identity))
            return false;

        var groupIds = identity.CsvToArrayWithoutEmpty()
            .Select(g => int.TryParse(g, out var gid) ? gid : 0)
            .Where(gid => gid > 0)
            .ToList();

        return user.Roles.Any(r => groupIds.Contains(r));
    }

    /// <summary>
    /// Verify that the permission is for owners, and the user is the item owner
    /// </summary>
    private static bool VerifyUserIsItemOwner(string condition, string targetOwner, IUser user)
        => condition == nameof(IPermission.Owner) && targetOwner == user.IdentityToken;
}