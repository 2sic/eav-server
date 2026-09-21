using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class MelLogFactory(ILoggerFactory loggerFactory) : ILogFactory
{
    public ILog Create(string name, ILog? parent, CodeRef code, string? initialMessage = default)
    {
        var logName = string.IsNullOrWhiteSpace(name) ? LogConstants.FullNameUnknown : name;
        var category = $"ToSic.{logName}";
        var log = new MelLog(loggerFactory.CreateLogger(category), this, category, logName);
        if (initialMessage != null)
            log.Add(initialMessage, code, default, LogEventKind.Trace);
        return log;
    }
}

internal enum LogEventKind
{
    Trace,
    Debug,
    Warning,
    Error
}

internal interface ILogEventSink
{
    void Add(string? message, CodeRef? code, EntryOptions? options, LogEventKind kind, Exception? exception = default);
}

internal interface ILogCallCompletionSink
{
    void Complete(string operation, string? completionMessage, object? result, bool hasResult, CodeRef code, long durationMilliseconds);
}

internal sealed class MelLog(ILogger logger, ILogFactory factory, string category, string logName) : ILog, ILogFactoryOwner, ILogEventSink, ILogCallCompletionSink
{
    public ILogFactory Factory { get; } = factory;

    public string NameId => category;

    public void Add(string? message, CodeRef? code, EntryOptions? options, LogEventKind kind, Exception? exception = default)
    {
        var level = kind switch
        {
            LogEventKind.Debug => LogLevel.Debug,
            LogEventKind.Warning => LogLevel.Warning,
            LogEventKind.Error => LogLevel.Error,
            _ => LogLevel.Trace
        };
        if (!logger.IsEnabled(level))
            return;

        // Keep only detached scalar values here, because MEL providers may process them after this call.
        var activity = Activity.Current;
        IReadOnlyList<KeyValuePair<string, object?>> state =
        [
            new("Message", message),
            new("LogName", logName),
            new("SourceFilePath", code?.Path),
            new("SourceMemberName", code?.Name),
            new("SourceLineNumber", code?.Line),
            new("HideCodeReference", options?.HideCodeReference ?? false),
            new("ShowNewLines", options?.ShowNewLines ?? false),
            new("TraceId", activity?.TraceId.ToString()),
            new("SpanId", activity?.SpanId.ToString()),
            new("{OriginalFormat}", "{Message}")
        ];
        logger.Log(level, default, state, exception, static (values, _) => values[0].Value?.ToString() ?? "");
    }

    public void Complete(string operation, string? completionMessage, object? result, bool hasResult, CodeRef code, long durationMilliseconds)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
            return;

        var activity = Activity.Current;
        var resultText = hasResult ? result?.ToString() ?? "null" : null;
        IReadOnlyList<KeyValuePair<string, object?>> state =
        [
            new("Message", string.IsNullOrWhiteSpace(completionMessage) ? operation : $"{operation} {completionMessage}"),
            new("LogName", logName),
            new("Operation", operation),
            new("CompletionMessage", completionMessage),
            new("Result", resultText),
            new("HasResult", hasResult),
            new("DurationMilliseconds", durationMilliseconds),
            new("SourceFilePath", code.Path),
            new("SourceMemberName", code.Name),
            new("SourceLineNumber", code.Line),
            new("TraceId", activity?.TraceId.ToString()),
            new("SpanId", activity?.SpanId.ToString()),
            new("{OriginalFormat}", "{Message}")
        ];
        logger.Log(LogLevel.Debug, default, state, null, static (values, _) => values[0].Value?.ToString() ?? "");
    }
}
