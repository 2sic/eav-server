using System.Runtime.CompilerServices;

namespace ToSic.Sys.Boot;

public static class BootLogHelper
{
    public static (ILog Main, ILogCall lStandalone, ILogCall lNormal)
        GetLoggersForStandaloneLogs(ILogStore logStore, ILog Log, string partName, string message, [CallerMemberName] string? cName = default)
    {
        // Create a new log for the standalone entry in the log store
        var standaloneLog = new Log($"B8T.{partName}", null, message);

        // Register the standalone log in the log store
        logStore.Add(LogNames.LogStoreStartUp, standaloneLog);

        // Start the standalone log with a timer
        var lStandalone = standaloneLog.Fn(timer: true, cName: cName);

        // Create a normal log for the same message, but without the standalone context
        var lNormal = Log.Fn(timer: true, cName: cName);

        // Return both for further use
        return (standaloneLog, lStandalone, lNormal);
    }

}