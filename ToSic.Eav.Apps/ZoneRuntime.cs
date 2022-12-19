using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps
{
    public class ZoneRuntime: ZoneBase
    {

        #region Constructor / DI

        public ZoneRuntime() : base("App.ZoneRt") { }

        protected ZoneRuntime(string logName = null): base(logName)
        {
        }

        #endregion
        

    }
}
