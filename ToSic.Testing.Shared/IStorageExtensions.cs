using ToSic.Eav.Apps.State;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

namespace ToSic.Testing.Shared;

public static  class IStorageExtensions
{
    public static IAppStateInternal AppStateReaderRawTA(this IRepositoryLoader storage, int appId) => storage.AppStateBuilderRaw(appId, new CodeRefTrail()).Reader;
}