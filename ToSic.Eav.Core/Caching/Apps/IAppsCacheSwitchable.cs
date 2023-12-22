using ToSic.Lib.DI;

namespace ToSic.Eav.Caching;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppsCacheSwitchable: IAppsCache, ISwitchableService
{
}