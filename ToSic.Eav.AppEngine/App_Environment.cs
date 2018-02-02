using System;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public partial class App
    {

        protected IEnvironment Env;
        public ITennant Tennant;

        protected App(IEnvironment env, int zoneId, int appId, ITennant tennant, bool allowSideEffects, Log parentLog) 
            : this(zoneId != AutoLookup    // if zone is missing, try to find it; if still missing, throw error
                  ? zoneId
                  : env.ZoneMapper.GetZoneId(tennant.Id), 
                  appId, 
                  allowSideEffects, 
                  parentLog,
                  $"P:{tennant?.Id}")
        {
            Env = env;

            Tennant = tennant ?? throw new Exception("no tennant (portal settings) received");

        }

        #region Paths
        protected string GetRootPath() => System.IO.Path.Combine(Tennant.SxcPath, Folder);

        public string PhysicalPath => Env.MapPath(GetRootPath());

        #endregion

    }
}
