﻿using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    /// <summary>
    /// Base class for other zone mappers.
    /// Has prepared code which should be the same across implementations. 
    /// </summary>
    public abstract class ZoneMapperBase: HasLog<IZoneMapper>, IZoneMapper
    {

        /// <summary>
        /// Trivial constructor
        /// </summary>
        protected ZoneMapperBase(IAppStates appStates, string logName) : base(logName)
        {
            AppStates = appStates;
        }
        protected readonly IAppStates AppStates;

        /// <inheritdoc />
        public abstract int GetZoneId(int siteId);

        /// <inheritdoc />
        public abstract ISite SiteOfZone(int zoneId);

        /// <inheritdoc />
        public ISite SiteOfApp(int appId)
        {
            var wrapLog = Log.Call<ISite>($"{appId}", "Must look for SiteId");
            var appIdentifier = AppStates.IdentityOfApp(appId);
            var tenant = SiteOfZone(appIdentifier.ZoneId);
            return wrapLog(null, tenant);
        }

        /// <inheritdoc />
        public abstract List<TempTempCulture> CulturesWithState(ISite site);
    }
}
