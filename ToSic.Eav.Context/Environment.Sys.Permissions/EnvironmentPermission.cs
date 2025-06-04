using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Sys;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;

namespace ToSic.Eav.Environment.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class EnvironmentPermission(string logPrefix, object[] connect = default)
    : ServiceBase($"{logPrefix}.EnvPrm", connect: connect), IEnvironmentPermission, IEnvironmentPermissionSetup
{
    // Constant keys for security, historic from Dnn
    protected const string SalPrefix = "SecurityAccessLevel";
    protected const string SalView = SalPrefix + ".View";
    protected const string SalAnonymous = SalPrefix + ".Anonymous";
    protected const string SalEdit = SalPrefix + ".Edit";
    protected const string SalSiteAdmin = SalPrefix + ".Admin";
    protected const string SalSystemUser = SalPrefix + ".Host";


    public IEnvironmentPermission Init<TContext>(IContextOfSite context, IAppIdentity appIdentityOrNull)
    {
        var siteCtx = context as IContextOfSite ?? throw new ArgumentException($"Must be an {nameof(IContextOfSite)}", nameof(context));
        Context = siteCtx;
        UserOrNull = siteCtx.User;
        SiteOrNull = siteCtx.Site;
        AppIdentityOrNull = appIdentityOrNull;
        return this;
    }
    protected IContextOfSite Context { get; private set; }
    private IUser? UserOrNull { get; set; }
    private ISite? SiteOrNull { get; set; }
    protected IAppIdentity AppIdentityOrNull { get; private set; }

    /// <summary>
    /// This should evaluate the grants and decide if the environment approves any of these grants.
    /// Note that in many cases the implementation will simply check if the environment provides edit permissions, but
    /// it can really check the grants required and compare each one with the environment.
    /// </summary>
    /// <param name="grants"></param>
    /// <returns></returns>
    protected virtual bool EnvironmentOk(List<Grants> grants)
    {
        var l = Log.Fn<bool>(Log.Try(() => $"[{string.Join(",", grants)}]"));
        if (UserIsAnonymous())
            return l.ReturnFalse("user anonymous");
        var ok = UserIsSystemAdmin(); // superusers are always ok
        if (!ok && CurrentZoneMatchesSiteZone())
            ok = UserIsContentAdmin()
                 || UserIsModuleAdmin()
                 || UserIsModuleEditor();
        return l.ReturnAndLog(ok);
    }

    public PermissionCheckInfo EnvironmentAllows(List<Grants> grants)
    {
        var ok = EnvironmentOk(grants);
        return new(ok, ok ? Conditions.EnvironmentGlobal : Conditions.Undefined);
    }

    protected virtual bool UserIsModuleAdmin() => false;
    protected virtual bool UserIsModuleEditor() => false;


    /// <summary>
    /// Verify if a condition is a special code in the environment. 
    /// Example: a DNN code which asks for "registered users" or "view-users"
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public abstract bool VerifyConditionOfEnvironment(string condition);


    /// <summary>
    /// Check if user is anonymous - also log the ID to assist in debugging
    /// </summary>
    /// <returns></returns>
    protected bool UserIsAnonymous()
        => Log.Quick(parameters: $"UserId:{UserOrNull?.Id.ToString()}", func: () => UserOrNull?.IsAnonymous ?? true);

    /// <summary>
    /// Check if user is super user
    /// </summary>
    /// <returns></returns>
    protected bool UserIsSystemAdmin()
        => Log.Quick(() => UserOrNull?.IsSystemAdmin ?? false);

    /// <summary>
    /// Check if user is valid admin of current portal / zone
    /// </summary>
    /// <returns></returns>
    protected bool UserIsContentAdmin()
        => Log.Quick(() => UserOrNull?.IsContentAdmin ?? false);

    /// <summary>
    /// Verify that we're in the same zone, allowing admin/module checks
    /// </summary>
    /// <returns></returns>
    protected bool CurrentZoneMatchesSiteZone()
    {
        var l = Log.Fn<bool>();
        // Check if we are running out-of http-context
        if (SiteOrNull == null || SiteOrNull.Id == EavConstants.NullId)
            return l.ReturnFalse("no");

        // Check if no app is provided, like when an app hasn't been selected yet, so it's an empty module, must be on current portal
        if (AppIdentityOrNull == null)
            return l.ReturnTrue("no app, so context unchanged");

        // If we have the full context, we must check if the site has changed
        // This will important for other security checks, only allow zone-change for super users
        var result = SiteOrNull.ZoneId == AppIdentityOrNull.ZoneId;
        return l.Return(result, result.ToString());
    }
}