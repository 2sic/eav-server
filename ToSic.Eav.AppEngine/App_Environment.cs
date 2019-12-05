using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Environment;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        [PrivateApi]
        protected IAppEnvironment Env;
        
        /// <inheritdoc />
        public ITenant Tenant { get; }

        [PrivateApi]
        protected App(ITenant tenant, int zoneId, int appId, bool allowSideEffects,
            Func<App, IAppDataConfiguration> buildConfiguration,
            ILog parentLog)
            : this(Factory.Resolve<IEnvironmentFactory>().Environment(parentLog), tenant, zoneId, appId,
                allowSideEffects, buildConfiguration, parentLog)
        {
        }

        [PrivateApi]
        protected App(IAppEnvironment env, 
            ITenant tenant, 
            int zoneId, 
            int appId, 
            bool allowSideEffects,
            Func<App, IAppDataConfiguration> buildConfiguration,
            ILog parentLog)
            : this(zoneId != AutoLookupZone    // if zone is missing, try to find it; if still missing, throw error
                  ? zoneId
                  : env.ZoneMapper.GetZoneId(tenant.Id), 
                  appId, 
                  allowSideEffects, 
                  buildConfiguration,
                  parentLog,
                  $"t#{tenant?.Id}")
        {
            Env = env ?? throw new Exception("no environment received");
            Tenant = tenant ?? throw new Exception("no tenant (portal settings) received");
        }

        #region Paths
        [PrivateApi]
        protected string GetRootPath() => System.IO.Path.Combine(Tenant.SxcPath, Folder);

        [PrivateApi]
        public string PhysicalPath => Env.MapAppPath(GetRootPath());

        #endregion

    }
}
