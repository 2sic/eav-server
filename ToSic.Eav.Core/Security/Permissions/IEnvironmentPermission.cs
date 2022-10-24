using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Security
{
    public interface IEnvironmentPermission: IHasLog<IEnvironmentPermission>
    {
        /// <summary>
        /// Init the checker
        /// </summary>
        /// <typeparam name="TContext">Important: Special type info for the context because the Eav.Core doesn't know about these types yet</typeparam>
        /// <param name="context"></param>
        /// <param name="appIdentityOrNull"></param>
        /// <returns></returns>
        IEnvironmentPermission Init<TContext>(TContext context, IAppIdentity appIdentityOrNull);

        Conditions GrantedBecause { get; set; }

        /// <summary>
        /// This should evaluate the grants and decide if the environment approves any of these grants.
        /// Note that in many cases the implementation will simply check if the environment provides edit permissions, but
        /// it can really check the grants required and compare each one with the environment.
        /// </summary>
        /// <param name="grants"></param>
        /// <returns></returns>
        bool EnvironmentAllows(List<Grants> grants);

        /// <summary>
        /// Verify if a condition is a special code in the environment. 
        /// Example: a DNN code which asks for "registered users" or "view-users"
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        bool VerifyConditionOfEnvironment(string condition);
    }
}
