using System.Collections.Generic;
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
        public IZoneMapper Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }

        /// <inheritdoc />
        public abstract int GetZoneId(int tenantId);

        /// <inheritdoc />
        public int GetZoneId(ITenant tenant) => GetZoneId(tenant.Id);

        /// <inheritdoc />
        public IAppIdentity IdentityFromTenant(int tenantId, int appId)
            => new AppIdentity(GetZoneId(tenantId), appId);

        /// <inheritdoc />
        public abstract ITenant TenantOfZone(int zoneId);

        /// <inheritdoc />
        public ITenant TenantOfApp(int appId)
        {
            var wrapLog = Log.Call<ITenant>($"{appId}");
            Log.Add("TenantId not found. Must be in search mode, will try to find correct portalsettings");
            var appIdentifier = State.Identity(null, appId);
            var tenant = TenantOfZone(appIdentifier.ZoneId);
            return wrapLog(null, tenant);
        }

        /// <inheritdoc />
        public abstract List<TempTempCulture> CulturesWithState(int tenantId, int zoneId);
    }
}
