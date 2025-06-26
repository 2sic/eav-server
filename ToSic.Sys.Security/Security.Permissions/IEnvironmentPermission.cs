namespace ToSic.Sys.Security.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IEnvironmentPermission: IHasLog
{
    /// <summary>
    /// This should evaluate the grants and decide if the environment approves any of these grants.
    /// Note that in many cases the implementation will simply check if the environment provides edit permissions, but
    /// it can really check the grants required and compare each one with the environment.
    /// </summary>
    /// <param name="grants"></param>
    /// <returns></returns>
    PermissionCheckInfo EnvironmentAllows(List<Grants> grants);

    /// <summary>
    /// Verify if a condition is a special code in the environment. 
    /// Example: a DNN code which asks for "registered users" or "view-users"
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    bool VerifyConditionOfEnvironment(string condition);
}