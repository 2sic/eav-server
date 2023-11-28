using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;

namespace ToSic.Eav.Apps.Reader
{
    public interface IAppStateInternal: IAppState, IHasPiggyBack
    {
        AppState AppState { get; }

        AppState ParentAppState { get; }


        public SynchronizedList<IEntity> ListPublished { get; }

        SynchronizedList<IEntity> ListNotHavingDrafts { get; }


        AppStateMetadata SettingsInApp { get; }

        AppStateMetadata ResourcesInApp { get; }
    }
}
