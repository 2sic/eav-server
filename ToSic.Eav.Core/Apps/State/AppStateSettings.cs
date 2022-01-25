using ToSic.Eav.Configuration;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {
        /// <summary>
        /// TODO: Warning - in rare cases this could be a problem
        /// Because we're storing an appStates indefinitely, which has a service provider etc. which was created at the time this app was created
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="target"></param>
        internal AppStateSettings(AppState owner, AppThingsIdentifiers target)
        {
            Owner = owner;
            Target = target;
        }

        private AppState Owner { get; }
        private AppThingsIdentifiers Target { get; }

    }
}
