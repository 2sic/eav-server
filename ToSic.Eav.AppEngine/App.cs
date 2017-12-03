using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps
{
    public class App: IAppIdentity
    {
        public int AppId { get; protected set; }
        public int ZoneId { get; protected set; }

    }
}
