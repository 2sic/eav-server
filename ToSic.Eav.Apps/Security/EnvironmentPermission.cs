using System;
using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;
using ToSic.Eav.Security;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Security;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class EnvironmentPermission : ServiceBase, IEnvironmentPermission
{
    // Constant keys for security, historic from Dnn
    protected const string SalPrefix = "SecurityAccessLevel";
    protected const string SalView = SalPrefix + ".View";
    protected const string SalAnonymous = SalPrefix + ".Anonymous";
    protected const string SalEdit = SalPrefix + ".Edit";
    protected const string SalSiteAdmin = SalPrefix + ".Admin";
    protected const string SalSystemUser = SalPrefix + ".Host";


    protected EnvironmentPermission(string logPrefix) : base($"{logPrefix}.EnvPrm") { }

    public IEnvironmentPermission Init<TContext>(TContext context, IAppIdentity appIdentityOrNull)
    {
        Context = context as IContextOfSite ?? throw new ArgumentException($"Must be an {nameof(IContextOfSite)}", nameof(context));
        AppIdentity = appIdentityOrNull;
        GrantedBecause = Conditions.Undefined;
        return this;
    }
    protected IContextOfSite Context { get; set; }
    protected IAppIdentity AppIdentity { get; set; }
    public Conditions GrantedBecause { get; set; }

    /// <summary>
    /// This should evaluate the grants and decide if the environment approves any of these grants.
    /// Note that in many cases the implementation will simply check if the environment provides edit permissions, but
    /// it can really check the grants required and compare each one with the environment.
    /// </summary>
    /// <param name="grants"></param>
    /// <returns></returns>
    public virtual bool EnvironmentAllows(List<Grants> grants) => Log.Func(Log.Try(() => $"[{string.Join(",", grants)}]"), () =>
    {
        if (UserIsAnonymous()) return (false, "user anonymous");
        var ok = UserIsSystemAdmin(); // superusers are always ok
        if (!ok && CurrentZoneMatchesSiteZone())
            ok = UserIsContentAdmin()
                 || UserIsModuleAdmin()
                 || UserIsModuleEditor();
        if (ok) GrantedBecause = Conditions.EnvironmentGlobal;
        return (ok, $"{ok} because:{GrantedBecause}");
    });

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
    protected bool UserIsAnonymous() => Log.Func($"UserId:{Context.User?.Id.ToString()}", () => Context.User?.IsAnonymous ?? true);

    /// <summary>
    /// Check if user is super user
    /// </summary>
    /// <returns></returns>
    protected bool UserIsSystemAdmin() => Log.Func(() => Context.User?.IsSystemAdmin ?? false);

    /// <summary>
    /// Check if user is valid admin of current portal / zone
    /// </summary>
    /// <returns></returns>
    protected bool UserIsContentAdmin() => Log.Func(() => Context.User?.IsContentAdmin ?? false);

    /// <summary>
    /// Verify that we're in the same zone, allowing admin/module checks
    /// </summary>
    /// <returns></returns>
    protected bool CurrentZoneMatchesSiteZone() => Log.Func(() =>
    {
        // Check if we are running out-of http-context
        if (Context.Site == null || Context.Site.Id == Constants.NullId) return (false, "no");

        // Check if no app is provided, like when an app hasn't been selected yet, so it's an empty module, must be on current portal
        if (AppIdentity == null) return (true, "no app, so context unchanged");

        // If we have the full context, we must check if the site has changed
        // This will important for other security checks, only allow zone-change for super users
        var result = Context.Site.ZoneId == AppIdentity.ZoneId;
        return (result, result.ToString());
    });
}