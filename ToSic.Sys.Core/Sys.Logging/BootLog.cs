namespace ToSic.Sys.Logging;

/// <summary>
/// Experimental special logger during booting to see if we have any extreme issues.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class BootLog
{
    public static ILog Log { get; } = Start();

    private static ILog Start() => Start(LogFactory.Create("Sys.BootLog"));

    internal static ILog Start(ILog log)
    {
        // Set the MEL segment before the first event, including events replayed after DI is ready.
        if (log is MelLog)
            new MelLogStore().Add("boot-log", log);
        log.A("Starting Boot Log");
        return log;
    }

    private static bool _addedToStore;

    public static void AddToStore(ILogStore store)
    {
        if (Log is MelLog)
            return; // MEL admission happened before the first boot event.
        if (_addedToStore) return;
        _addedToStore = true;
        store.Add("boot-log", Log);
    }
}
