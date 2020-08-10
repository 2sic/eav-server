using ToSic.Eav.Documentation;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        [PrivateApi]
        protected IAppEnvironment Env;
        
        /// <inheritdoc />
        public ITenant Tenant { get; protected set; }

        //[PrivateApi]
        //protected App(ITenant tenant, int zoneId, int appId, bool allowSideEffects,
        //    Func<App, IAppDataConfiguration> buildConfiguration,
        //    ILog parentLog)
        //    : this(Factory.Resolve<IAppEnvironment>().Init(parentLog), tenant, zoneId, appId,
        //        allowSideEffects, buildConfiguration, parentLog)
        //{
        //}

        //[PrivateApi]
        //protected App Init(
        //    //    IAppEnvironment env, 
        //    //ITenant tenant, 
        //    int zoneId, 
        //    int appId, 
        //    bool allowSideEffects,
        //    Func<App, IAppDataConfiguration> buildConfiguration,
        //    ILog parentLog)
        //    //: this(zoneId != AutoLookupZone    // if zone is missing, try to find it; if still missing, throw error
        //    //      ? zoneId
        //    //      : env.ZoneMapper.GetZoneId(tenant.Id), 
        //    //      appId, 
        //    //      allowSideEffects, 
        //    //      buildConfiguration,
        //    //      parentLog,
        //    //      $"t#{tenant?.Id}")
        //{
        //    Env = Env ?? throw new Exception("no environment received");
        //    Tenant = Tenant ?? throw new Exception("no tenant (portal settings) received");

        //    Env.Init(parentLog);
        //    Init(new AppIdentity(
        //            zoneId != AutoLookupZone // if zone is missing, try to find it; if still missing, throw error
        //                ? zoneId
        //                : Env.ZoneMapper.GetZoneId(Tenant.Id),
        //            appId),
        //        allowSideEffects,
        //        buildConfiguration,
        //        parentLog,
        //        $"t#{Tenant?.Id}");

        //    return this;
        //}

        #region Paths
        [PrivateApi]
        protected string GetRootPath() => System.IO.Path.Combine(Tenant.SxcPath, Folder);

        [PrivateApi]
        public string PhysicalPath => Env.MapPath(GetRootPath());

        #endregion

    }
}
