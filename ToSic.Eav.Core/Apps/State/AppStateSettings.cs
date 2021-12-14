using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {

        public AppState Parent { get; }

        /// <summary>
        /// TODO: Warning - in rare cases this could be a problem
        /// Because we're storing an appStates indefinitely, which has a service provider etc. which was created at the time this app was created
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="appStates"></param>
        internal AppStateSettings(AppState parent, IAppStates appStates)
        {
            Parent = parent;

            var site = appStates.Get(appStates.IdentityOfPrimary(Parent.ZoneId));
            var global = appStates.Get(Constants.GlobalIdentity);

            Stacks = ConfigurationConstants.AppThingsArray
                .Select(at => new AppStateStackCache(parent, site, global, at))
                .ToArray();
        }

        public AppStateStackCache[] Stacks;

        public AppStateStackCache Get(AppThingsToStack target) => Stacks.First(s => s.Target.Target == target);

    }
}
