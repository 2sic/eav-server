using System.Diagnostics;
using static System.String;

namespace ToSic.Sys.Logging;

[PrivateApi("no need to publish this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LogCallBase : ILogCall
{
    private readonly ILogCallCompletionSink? _completionSink;
    private readonly string? _operation;
    private readonly CodeRef _code;
    private int _completed;

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

        var openingMessage = $"{code.Name}{(isProperty ? "" : $"({parameters})")}";
        if (!IsNullOrWhiteSpace(message))
            openingMessage += (IsNullOrWhiteSpace(openingMessage) ? "" : " ") + $"{message}";

        // Keep the log, but quit if it's not valid
        switch (log.GetRealLog())
        {
            case Log typedLog:
                Log = typedLog;
                var entry = Entry = Log.AddInternalReuse(openingMessage, code);
                entry.WrapOpen = true;
                typedLog.WrapDepth++;
                break;
            case MelLog melLog:
                // MEL keeps one completion event instead of recreating the Legacy open/close Entry pair.
                Log = melLog;
                _completionSink = melLog;
                _operation = openingMessage;
                _code = code;
                break;
        }
    }

    internal void Complete(string? message, object? result, bool hasResult)
    {
        // Keep Legacy first, because its repeated completion behavior is part of the existing diagnostics.
        if (Log is Log log)
        {
            log.WrapDepth--;
            Entry?.AppendResult(message);
            var final = log.AddInternalReuse(null!, null);
            final.WrapClose = true;
            final.AppendResult(message);
            if (!Timer.IsRunning)
                return;
            Timer.Stop();
            if (Entry != null)
                Entry.Elapsed = Timer.Elapsed;
            return;
        }

        // Existing client code can complete a call more than once; MEL should publish it only once.
        if (Interlocked.Exchange(ref _completed, 1) != 0)
            return;

        if (Timer.IsRunning)
            Timer.Stop();
        _completionSink?.Complete(_operation!, message, result, hasResult, _code, Timer.ElapsedMilliseconds);
    }

    public ILog? Log { get; }

    /// <inheritdoc />
    public Entry? Entry { get; }

    /// <inheritdoc />
    public Stopwatch Timer { get; }

    [PrivateApi("will probably remove")]
    public string NameId => Log?.NameId ?? "no-name";

}
