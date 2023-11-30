using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps
{
    public interface IAppStateCache: ICacheExpiring
    {
        AppRelationshipManager Relationships { get; }
    }
}
