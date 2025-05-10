using System.Diagnostics;
using static System.String;

namespace ToSic.Lib.Logging;

[PrivateApi("no need to publish this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogCallBase : ILogLike, ILogCall
{
    /// <summary>
    /// Keep constructor internal
    /// </summary>
    [PrivateApi]
    internal LogCallBase(ILog? log,
        CodeRef code,
        bool isProperty,
        string? parameters = null,
        string? message = null,
        bool timer = false)
    {
        // Always init the stopwatch, as it could be used later even without a parent log
        Timer = timer
            ? Stopwatch.StartNew()
            : new();

        // Keep the log, but quit if it's not valid
        if (log.GetRealLog() is not Log typedLog)
            return;
        Log = typedLog;

        var openingMessage = $"{code.Name}{(isProperty ? "" : $"({parameters})")}";
        if (!IsNullOrWhiteSpace(message)) 
            openingMessage += (IsNullOrWhiteSpace(openingMessage) ? "" : " ") + $"{message}";
        var entry = Entry = Log.AddInternalReuse(openingMessage, code);
        entry.WrapOpen = true;
        typedLog.WrapDepth++;
    }

    public ILog? Log { get; }

    /// <inheritdoc />
    public Entry? Entry { get; }

    /// <inheritdoc />
    public Stopwatch Timer { get; }

    [PrivateApi("will probably remove")]
    public string NameId => Log?.NameId ?? "no-name";

}