using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Users;
using ToSic.Sys.Utils;

namespace ToSic.Sys.Security.Permissions;

partial class PermissionCheck
{
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
            l.A($"condition:{permission.Condition}, identity:{permission.Identity}");

            // check custom permission based on the user Guid or owner
            if (User.Guid != default)
            {
                // check owner conditions (only possible on target entities, not content-types)
                if (VerifyUserIsItemOwner(permission.Condition, MyOptions.TargetOwner, User))
                    return l.Return(new(true, Conditions.Identity), "is-owner: true");

                // check if an identity was provided
                if (!string.IsNullOrWhiteSpace(permission.Identity))
                {
                    l.A($"Check if user is user or group - identity: {permission.Identity}");
                    if (VerifyUserIsThisUser(permission.Identity, User))
                        return l.Return(new(true, Conditions.Owner), "is-this-user: true");

                    if (VerifyUserIsInGroup(permission.Identity, User))
                        return l.Return(new(true, Conditions.Group), "is-in-specified-group: true");
                }
            }

            // this checks if the condition is an environment condition
            // for example, if it's a DNN code for "user may view something"
            return EnvPermissions.VerifyConditionOfEnvironment(permission.Condition)
                ? l.Return(new(true, Conditions.EnvironmentInstance), "environment: true")
                : l.Return(new (false, Conditions.Undefined), "no-match: false");
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
        if (!featuresSvc.IsEnabled(BuiltInFeatures.PermissionCheckUsers.Guid))
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
        if (!featuresSvc.IsEnabled(BuiltInFeatures.PermissionCheckUsers.Guid))
            return false;

        if (string.IsNullOrWhiteSpace(identity))
            return false;

        var groupIds = identity.CsvToArrayWithoutEmpty()
            .Select(g => int.TryParse(g, out var gid) ? gid : 0)
            .Where(gid => gid > 0)
            .ToList();

        return user.Roles.Any(groupIds.Contains);
    }

    /// <summary>
    /// Verify that the permission is for owners, and the user is the item owner
    /// </summary>
    private static bool VerifyUserIsItemOwner(string condition, string? targetOwner, IUser user)
        => condition == nameof(IPermission.Owner) && targetOwner == user.IdentityToken;
}