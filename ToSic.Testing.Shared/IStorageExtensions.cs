using ToSic.Eav.Apps;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

namespace ToSic.Testing.Shared
{
    public static  class IStorageExtensions
    {
        public static AppState AppStateRawTA(this IRepositoryLoader storage, int appId) => storage.AppStateRaw(appId, new CodeRef());
    }
}
