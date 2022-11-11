using ToSic.Lib.Logging;

namespace ToSic.Eav.WebApi
{
    /// <summary>
    /// Temporary class to build the Real infrastructure before all controllers support it.
    /// Created 2022-02
    /// Probably remove again once we're done migrating - see https://r.2sxc.org/proxy-controllers
    /// </summary>
    public class DummyControllerReal: HasLog
    {
        public DummyControllerReal() : base("") { }
    }
}
