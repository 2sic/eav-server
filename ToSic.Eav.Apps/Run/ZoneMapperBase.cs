using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    /// <summary>
    /// Base class for other zone mappers.
    /// Has prepared code which should be the same across implementations. 
    /// </summary>
    public abstract class ZoneMapperBase: HasLog, IZoneMapper
    {
        /// <summary>
        /// Trivial constructor
        /// </summary>
        /// <param name="logName"></param>
        protected ZoneMapperBase(string logName) : base(logName) { }

        /// <inheritdoc />
        public IZoneMapper Init(ILog parentLog)
        {
            Log.LinkTo(parentLog);
            return this;
        }

        /// <inheritdoc />
        public abstract int GetZoneId(int siteId);

        /// <inheritdoc />
        public int GetZoneId(ISite site) => GetZoneId(site.Id);

        ///// <inheritdoc />
        //public IAppIdentity IdentityFromSite(int tenantId, int appId)
        //    => new AppIdentity(GetZoneId(tenantId), appId);

        /// <inheritdoc />
        public abstract ISite SiteOfZone(int zoneId);

        /// <inheritdoc />
        public ISite TenantOfApp(int appId)
        {
            var wrapLog = Log.Call<ISite>($"{appId}");
            Log.Add("TenantId not found. Must be in search mode, will try to find correct portalsettings");
            var appIdentifier = State.Identity(null, appId);
            var tenant = SiteOfZone(appIdentifier.ZoneId);
            return wrapLog(null, tenant);
        }

        /// <inheritdoc />
        public abstract List<TempTempCulture> CulturesWithState(int tenantId, int zoneId);
    }
}
