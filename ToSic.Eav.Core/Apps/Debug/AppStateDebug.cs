using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps.Debug
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class AppStateDebug
    {
        public static SynchronizedEntityList ListCache(this AppState appState)
            => appState.ListCache;
    }
}
