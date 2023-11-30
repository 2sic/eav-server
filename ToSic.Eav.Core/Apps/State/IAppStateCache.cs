using System;
using ToSic.Eav.Caching;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Metadata;
using ToSic.Lib.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    public interface IAppStateCache: ICacheExpiring, IHasMetadata, IHasPiggyBack, IAppIdentity, IHasMetadataSource, IHasIdentityNameId, 
        IEntitiesSource, IHasLog
    {

        string Folder { get; }

        AppRelationshipManager Relationships { get; }

        AppStateMetadata ThingInApp(AppThingsToStack target);

        ParentAppState ParentApp { get; }

        ICacheStatistics CacheStatistics { get; }

        int DynamicUpdatesCount { get; }

        void PreRemove();

        void DoInLock(ILog parentLog, Action transaction);
    }
}
