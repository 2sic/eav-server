using ToSic.Eav.Sys;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Environment.Sys.Permissions;

/// <summary>
/// Reusable Environment-Permissions logic, to be used by the specific implementation.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class EnvironmentPermissionsLogic(ILog parentLog): HelperBase(parentLog, "Sec.EnvPHl")
{

    /// <summary>
    /// This should evaluate the grants and decide if the environment approves any of these grants.
    /// Note that in many cases the implementation will simply check if the environment provides edit permissions, but
    /// it can really check the grants required and compare each one with the environment.
    /// </summary>
    /// <param name="grants">The list of grants to check.</param>
    /// <param name="options">The environment permission options.</param>
    /// <param name="userIsModuleAdmin">Function to check if the user is a module admin.</param>
    /// <param name="userIsModuleEditor">Function to check if the user is a module editor.</param>
    /// <returns>True if the environment approves the grants, otherwise false.</returns>
    public bool EnvironmentAllows(List<Grants> grants, EnvironmentPermissionOptions options, Func<bool> userIsModuleAdmin, Func<bool> userIsModuleEditor)
    {
        var l = Log.Fn<bool>(Log.Try(() => $"[{string.Join(",", grants)}]"));
        if (UserIsAnonymous())
            return l.ReturnFalse("user anonymous");
        var ok = UserIsSystemAdmin(); // superusers are always ok
        if (!ok && CurrentZoneMatchesSiteZone())
            ok = UserIsContentAdmin()
                 || userIsModuleAdmin()
                 || userIsModuleEditor();
        return l.ReturnAndLog(ok);

        // <summary>
        // Check if user is anonymous - also log the ID to assist in debugging
        // </summary>
        // <returns></returns>
        bool UserIsAnonymous() => Log.Quick(parameters: $"UserId:{options.User.Id.ToString()}", func: () => options.User.IsAnonymous);

        // <summary>
        // Check if user is superuser
        // </summary>
        // <returns></returns>
        bool UserIsSystemAdmin() => Log.Quick(() => options.User.IsSystemAdmin);

        // <summary>
        // Check if user is valid admin of current portal / zone
        // </summary>
        // <returns></returns>
        bool UserIsContentAdmin() => Log.Quick(() => options.User.IsContentAdmin);

        // <summary>
        // Verify that we're in the same zone, allowing admin/module checks
        // </summary>
        // <returns></returns>
        bool CurrentZoneMatchesSiteZone()
        {
            var l2 = Log.Fn<bool>();
            // Check if we are running out-of http-context
            if (options.Site.Id == EavConstants.NullId)
                return l2.ReturnFalse("no");

            // Check if no app is provided, like when an app hasn't been selected yet, so it's an empty module, must be on current portal
            if (options.AppIdentityOrNull == null)
                return l2.ReturnTrue("no app, so context unchanged");

            // If we have the full context, we must check if the site has changed
            // This will important for other security checks, only allow zone-change for superusers
            var result = options.Site.ZoneId == options.AppIdentityOrNull.ZoneId;
            return l2.Return(result, result.ToString());
        }
    }
}