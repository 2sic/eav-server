﻿using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {

        public AppState Parent { get; }

        internal AppStateSettings(AppState parent, IAppStates appStates)
        {
            Parent = parent;

            Stacks = ConfigurationConstants.AppThingsArray
                .Select(at => new AppStateStackCache(parent, at, appStates))
                .ToArray();
        }

        public AppStateStackCache[] Stacks;

        public AppStateStackCache Get(AppThingsToStack target) => Stacks.First(s => s.Target.Target == target);

    }
}
