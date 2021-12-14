﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;
using static ToSic.Eav.Configuration.ConfigurationStack;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {
        public List<KeyValuePair<string, IPropertyLookup>> GetStack(IServiceProvider sp, IEntity viewPart)
        {
            // View level - always add, no matter if null
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                new KeyValuePair<string, IPropertyLookup>(PartView, viewPart)
            };

            // All in the App and below
            sources.AddRange(Get(sp).FullStack());
            return sources;
        }


        private AppStateStackCache _stackCache;

        private AppStateStackCache Get(IServiceProvider sp)
        {
            if (_stackCache != null) return _stackCache;

            // Not yet, so we must build the stack
            var appStates = sp.Build<IAppStates>();
            var site = appStates.Get(appStates.IdentityOfPrimary(Parent.ZoneId));
            var global = appStates.Get(Constants.GlobalIdentity);
            var preset = appStates.Get(Constants.PresetIdentity);

            _stackCache = new AppStateStackCache(Parent, site, global, preset, Target);

            return _stackCache;
        }
    }
}
