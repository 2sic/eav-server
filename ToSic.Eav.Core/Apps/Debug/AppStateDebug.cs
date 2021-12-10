using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps.Debug
{
    public static class AppStateDebug
    {
        public static SynchronizedEntityList ListCache(this AppState appState)
            => appState.ListSyncInternal;
    }
}
