using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    public abstract class ZoneBase : ServiceBase, IZoneIdentity
    {
        #region Constructor and simple properties

        public int ZoneId { get; internal set; }

        protected ZoneBase(string logName): base(logName) { }

        #endregion
    }

    public static class ZoneBaseExtensions
    {
        public static T SetId<T>(this T parent, int zoneId) where T : ZoneBase => parent.Log.WrpFn<T>(_ =>
        {
            parent.ZoneId = zoneId;
            return parent;
        });
    }
}
