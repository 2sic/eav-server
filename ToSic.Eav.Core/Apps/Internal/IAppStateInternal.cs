using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;

namespace ToSic.Eav.Apps.Reader
{
    public interface IAppStateInternal: IAppState, IHasPiggyBack, IMetadataOfSource
    {
        IAppStateCache StateCache { get; }

        IAppStateCache ParentAppState { get; }

        SynchronizedEntityList ListCache { get; }

        public SynchronizedList<IEntity> ListPublished { get; }

        SynchronizedList<IEntity> ListNotHavingDrafts { get; }


        AppStateMetadata SettingsInApp { get; }

        AppStateMetadata ResourcesInApp { get; }

        IEntity ConfigurationEntity { get; }

        IContentType GetContentType(int contentTypeId);

        ParentAppState ParentApp { get; }
    }
}
