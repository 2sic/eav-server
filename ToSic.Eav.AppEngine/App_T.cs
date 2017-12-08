using System;
using System.Linq;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public class App<T>: App
    {

        protected IEnvironment<T> Env;
        public Tennant<T> Tennant;

        protected App(IEnvironment<T> env, int zoneId, int appId, Tennant<T> tennant, bool allowSideEffects, Log parentLog) 
            : base(zoneId != AutoLookup    // if zone is missing, try to find it; if still missing, throw error
                  ? zoneId
                  : env.ZoneMapper.GetZoneId(tennant), 
                  appId, 
                  allowSideEffects, 
                  parentLog,
                  $"P:{tennant?.Id}")
        {
            Env = env;

            Tennant = tennant ?? throw new Exception("no tennant (portal settings) received");

        }

        #region Paths
        protected string GetRootPath() => System.IO.Path.Combine(Tennant.RootPath, Folder);

        public string PhysicalPath => Env.MapPath(GetRootPath());

        #endregion

        #region Data

        /// <summary>
        /// Override and enhance with environment data like current user, languages, etc.
        /// </summary>
        /// <returns></returns>
        protected override DataSources.App BuildData()
        {
            var xData = base.BuildData();
            var languagesActive = Env.ZoneMapper.CulturesWithState(Tennant.Settings, ZoneId)
                .Any(c => c.Active);
            xData.DefaultLanguage = languagesActive
                ? Tennant.DefaultLanguage
                : "";
            xData.CurrentUserName = Env.User.CurrentUserIdentityToken;

            return xData;
        }

        #endregion
    }
}
