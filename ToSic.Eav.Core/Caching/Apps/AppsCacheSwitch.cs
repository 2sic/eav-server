using ToSic.Eav.Plumbing;
using ToSic.Eav.Plumbing.DI;

namespace ToSic.Eav.Caching
{
    public class AppsCacheSwitch : ServiceSwitcherSingleton<IAppsCacheSwitchable>
    {
        public AppsCacheSwitch(LazyInitLog<ServiceSwitcher<IAppsCacheSwitchable>> serviceSwitcher) : base(serviceSwitcher)
        {
        }
    }
}
