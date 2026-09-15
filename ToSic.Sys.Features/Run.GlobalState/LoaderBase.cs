namespace ToSic.Sys.Run.GlobalState;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LoaderBase: ServiceBase
{
    public LoaderBase(ILogStore logStore, string logName, object[]? connect = default) : base(logName)
    {
        logStore.Add(LogNames.LogStoreStartUp, Log);
    }
}