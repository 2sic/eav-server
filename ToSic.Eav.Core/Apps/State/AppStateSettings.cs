using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppSettingsStack: HasLog
    {

        public AppSettingsStack(IAppStates appStates): base("App.Stack")
        {
            _appStates = appStates;
        }

        private readonly IAppStates _appStates;

        /// <summary>
        /// </summary>
        /// <param name="owner"></param>
        public AppSettingsStack Init(AppState owner)
        {
            Owner = owner;
            return this;
        }

        private AppState Owner { get; set; }
    }
}
