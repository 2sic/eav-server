using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public abstract class EnvironmentPermission : HasLog, IEnvironmentPermission
    {
        protected EnvironmentPermission(string logName) : base(logName)
        {
        }

        public IEnvironmentPermission Init<T>(T ctx, IAppIdentity appIdentity, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            Context = ctx as IContextOfSite;
            AppIdentity = appIdentity;
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
        protected bool UserIsSuperuser() => Log.Intercept(nameof(UserIsSuperuser), () => Context.User?.IsSuperUser ?? false);

        /// <summary>
        /// Check if user is valid admin of current portal / zone
        /// </summary>
        /// <returns></returns>
        protected bool UserIsSiteAdmin() => Log.Intercept(nameof(UserIsSiteAdmin), () => Context.User?.IsAdmin ?? false);


        /// <summary>
        /// Verify that we're in the same zone, allowing admin/module checks
        /// </summary>
        /// <returns></returns>
        protected bool CurrentZoneMatchesSiteZone()
        {
            var wrapLog = Log.Call<bool>();
            // but is the current portal also the one we're asking about?
            if (Context.Site == null || Context.Site.Id == Eav.Constants.NullId) return wrapLog("no", false); // this is the case when running out-of http-context
            if (AppIdentity == null) return wrapLog("yes", true); // this is the case when an app hasn't been selected yet, so it's an empty module, must be on current portal
            var pZone = Context.Site.ZoneId;
            var result = pZone == AppIdentity.ZoneId; // must match, to accept user as admin
            return wrapLog($"{result}", result);
        }
    }
}