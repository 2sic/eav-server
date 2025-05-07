using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging;

/// <summary>
/// WIP - special class to reduce logging noise
/// </summary>
public class LogFilter(
    ILog log,
    int? logFirstMax = LogFilter.LogFirstMaxDefault,
    int? reLogIteration = LogFilter.ReLogIterationDefault
)
{
    public const int LogFirstMaxDefault = 25;
    public const int ReLogIterationDefault = 100;

    public int LogFirstMax { get; } = logFirstMax ?? LogFirstMaxDefault;
    public int ReLogIteration { get; } = reLogIteration ?? ReLogIterationDefault;

    public int Count { get; private set; }

    public bool ShouldLog => Count <= LogFirstMax || Count % ReLogIteration == 0;

    public ILogCall<T> FnOrNull<T>(
        string parameters = default,
        string message = default,
        bool timer = false,
        bool enabled = true,
        [CallerFilePath] string cPath = default,
        [CallerMemberName] string cName = default,
        [CallerLineNumber] int cLine = default
    )
    {
        Count++;
        message += $" call {Count}";
        if (Count == LogFirstMax)
            message += $" - after this, logging will be reduced to every {ReLogIteration}th call";
        // ReSharper disable ExplicitCallerInfoArgument
        return (ShouldLog ? log : null).Fn<T>(parameters, message, timer, enabled, cPath, cName, cLine);
        // ReSharper restore ExplicitCallerInfoArgument
    }
}