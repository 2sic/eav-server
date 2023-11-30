using ToSic.Eav.Caching;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    public interface IAppStateCache: ICacheExpiring, IAppStateFullList, IHasMetadata
    {
        AppRelationshipManager Relationships { get; }
    }
}
