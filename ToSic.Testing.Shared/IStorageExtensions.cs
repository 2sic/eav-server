using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Reader;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

namespace ToSic.Testing.Shared
{
    public static  class IStorageExtensions
    {
        public static AppState AppStateRawTA(this IRepositoryLoader storage, int appId) => storage.AppStateBuilderRaw(appId, new CodeRefTrail()).AppState;
        public static IAppStateInternal AppStateReaderRawTA(this IRepositoryLoader storage, int appId) => storage.AppStateBuilderRaw(appId, new CodeRefTrail()).Reader;
    }
}
