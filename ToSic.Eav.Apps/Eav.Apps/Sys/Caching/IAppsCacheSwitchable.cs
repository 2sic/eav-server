using ToSic.Lib.DI;

namespace ToSic.Eav.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppsCacheSwitchable: IAppsCache, ISwitchableService;