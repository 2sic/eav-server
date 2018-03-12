using System;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public partial class App
    {

        protected IEnvironment Env;
        public ITenant Tenant;

        protected App(IEnvironment env, int zoneId, int appId, ITenant tenant, bool allowSideEffects, Log parentLog) 
            : this(zoneId != AutoLookup    // if zone is missing, try to find it; if still missing, throw error
                  ? zoneId
                  : env.ZoneMapper.GetZoneId(tenant.Id), 
                  appId, 
                  allowSideEffects, 
                  parentLog,
                  $"P:{tenant?.Id}")
        {
            Env = env;

            Tenant = tenant ?? throw new Exception("no tenant (portal settings) received");

        }

        #region Paths
        protected string GetRootPath() => System.IO.Path.Combine(Tenant.SxcPath, Folder);

        public string PhysicalPath => Env.MapPath(GetRootPath());

        #endregion

    }
}
