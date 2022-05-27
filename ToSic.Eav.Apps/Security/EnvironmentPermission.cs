using System;
using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public abstract class EnvironmentPermission : HasLog<IEnvironmentPermission>, IEnvironmentPermission
    {
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
        public abstract bool EnvironmentAllows(List<Grants> grants);

        /// <summary>
        /// Verify if a condition is a special code in the environment. 
        /// Example: a DNN code which asks for "registered users" or "view-users"
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public abstract bool VerifyConditionOfEnvironment(string condition);


        /// <summary>
        /// Check if user is super user
        /// </summary>
        /// <returns></returns>
        protected bool UserIsSuperuser() => Log.Return(() => Context.User?.IsSuperUser ?? false);

        /// <summary>
        /// Check if user is valid admin of current portal / zone
        /// </summary>
        /// <returns></returns>
        protected bool UserIsSiteAdmin() => Log.Return(() => Context.User?.IsAdmin ?? false);


        /// <summary>
        /// Verify that we're in the same zone, allowing admin/module checks
        /// </summary>
        /// <returns></returns>
        protected bool CurrentZoneMatchesSiteZone()
        {
            var wrapLog = Log.Fn<bool>();
            
            // Check if we are running out-of http-context
            if (Context.Site == null || Context.Site.Id == Eav.Constants.NullId) return wrapLog.ReturnFalse("no");
            
            // Check if no app is provided, like when an app hasn't been selected yet, so it's an empty module, must be on current portal
            if (AppIdentity == null) return wrapLog.ReturnTrue("no app, so context unchanged"); 

            // If we have the full context, we must check if the site has changed
            // This will important for other security checks, only allow zone-change for super users
            var result = Context.Site.ZoneId == AppIdentity.ZoneId;
            return wrapLog.ReturnAndLog(result);
        }
    }
}