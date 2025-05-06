using ToSic.Lib.Logging;

namespace ToSic.Eav.StartUp;

/// <summary>
/// Experimental special logger during booting to see if we have any extreme issues.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class BootLog
{
    public static ILog Log { get; } = Start();

    private static ILog Start()
    {
        var log = new Log("Sys.BootLog");
        log.A("Starting Boot Log");
        return log;
    }

    private static bool _addedToStore;

    public static void AddToStore(ILogStore store)
    {
        if (_addedToStore) return;
        _addedToStore = true;
        store.Add("boot-log", Log);
    }
}