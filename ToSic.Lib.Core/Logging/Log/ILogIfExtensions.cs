namespace ToSic.Lib.Logging;

public static class ILogIfExtensions
{
    /// <summary>
    /// Conditionally returns the log if the condition is true.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    /// <remarks>
    /// Usually in Log.Condition(value).Fn(...) chaining to conditionally disable logging.
    /// </remarks>
    public static ILog If(this ILog log, bool condition)
        => condition
            ? log
            : null;

    public static ILog If(this ILog log, LogSettings settings)
        => (settings?.Enabled ?? false)
            ? log
            : null;

    public static ILog IfDetails(this ILog log, LogSettings settings)
        => settings is { Enabled: true, Details: true }
            ? log
            : null;

    public static ILog IfSummary(this ILog log, LogSettings settings)
        => settings is { Enabled: true, Summary: true }
            ? log
            : null;

}