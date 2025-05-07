using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders;

public class LoaderBase: ServiceBase
{
    public LoaderBase(ILogStore logStore, string logName, object[]? connect = default) : base(logName, connect: connect ?? [])
    {
        logStore.Add(LogNames.LogStoreStartUp, Log);
    }
}