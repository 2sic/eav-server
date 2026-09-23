namespace ToSic.Sys.Logging;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MelLogStore : ILogStore
{
    public LogStoreEntry? Add(string segment, ILog log)
        => AddInternal(segment, log);

    public LogStoreEntry? ForceAdd(string key, ILog log)
        => AddInternal(key, log);

    private static LogStoreEntry? AddInternal(string segment, ILog log)
    {
        if (log.GetRealLog() is not MelLog melLog)
            throw new InvalidOperationException("The MEL store only accepts MEL logs.");

        // Set this before more events are written, so segment flush also removes normal log lines.
        melLog.SetSegment(segment);

        // The entry stays a caller-owned metadata handle; MEL retains no Legacy store entry or log graph.
        return new()
        {
            Log = melLog,
            SpecsChanged = specs => melLog.AddSpecs(segment, specs)
        };
    }
}
