using System;
using ToSic.Eav.Configuration;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {

        public AppState Parent { get; }
        public AppThingsIdentifiers Target { get; }

        /// <summary>
        /// TODO: Warning - in rare cases this could be a problem
        /// Because we're storing an appStates indefinitely, which has a service provider etc. which was created at the time this app was created
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        internal AppStateSettings(AppState parent, AppThingsIdentifiers target)
        {
            Parent = parent;
            Target = target;
        }


    }
}
