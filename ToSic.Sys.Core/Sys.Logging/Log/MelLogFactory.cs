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
    Warning,
    Error
}

internal interface ILogEventSink
{
    void Add(string? message, CodeRef? code, EntryOptions? options, LogEventKind kind, Exception? exception = default);
}

internal sealed class MelLog(ILogger logger, ILogFactory factory, string category, string logName) : ILog, ILogFactoryOwner, ILogEventSink
{
    public ILogFactory Factory { get; } = factory;

    public string NameId => category;

    public void Add(string? message, CodeRef? code, EntryOptions? options, LogEventKind kind, Exception? exception = default)
    {
        var level = kind switch
        {
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
}
