using System;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public partial class App
    {

        protected IEnvironment Env;
        public ITenant Tenant;

        public App(ITenant tenant, int zoneId, int appId, bool allowSideEffects,
            Func<App, IAppDataConfiguration> buildConfiguration,
            Log parentLog)
            : this(Factory.Resolve<IEnvironmentFactory>().Environment(parentLog), tenant, zoneId, appId,
                allowSideEffects, buildConfiguration, parentLog)
        {
        }

        protected App(IEnvironment env, 
            ITenant tenant, 
            int zoneId, 
            int appId, 
            bool allowSideEffects,
            Func<App, IAppDataConfiguration> buildConfiguration,
            Log parentLog)
            : this(zoneId != AutoLookupZone    // if zone is missing, try to find it; if still missing, throw error
                  ? zoneId
                  : env.ZoneMapper.GetZoneId(tenant.Id), 
                  appId, 
                  allowSideEffects, 
                  buildConfiguration,
                  parentLog,
                  $"P:{tenant?.Id}")
        {
            Env = env ?? throw new Exception("no environment received");
            Tenant = tenant ?? throw new Exception("no tenant (portal settings) received");
        }

        #region Paths
        protected string GetRootPath() => System.IO.Path.Combine(Tenant.SxcPath, Folder);

        public string PhysicalPath => Env.MapPath(GetRootPath());

        #endregion

    }
}
