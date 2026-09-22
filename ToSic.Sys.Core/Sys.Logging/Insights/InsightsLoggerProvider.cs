using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class InsightsLoggerProvider(IInsightsLogStore store) : ILoggerProvider
{
    private long _sequence;

    public ILogger CreateLogger(string categoryName)
        => new InsightsLogger(this, categoryName);

    public void Dispose() { }

    private void Append<TState>(string category, LogLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsAccepted(category))
            return;

        try
        {
            // Snapshot everything now; the store must not retain MEL state, Activity or Exception objects.
            var values = Values(state);
            var activity = Activity.Current;
            var traceId = Value(values, "TraceId") ?? activity?.TraceId.ToString();
            var spanId = Value(values, "SpanId") ?? activity?.SpanId.ToString();
            var sequence = Interlocked.Increment(ref _sequence);
            store.Append(new()
            {
                Sequence = sequence,
                TimestampUtc = DateTime.UtcNow,
                Category = category,
                Level = Level(level),
                EventId = eventId.Id,
                EventName = eventId.Name,
                Message = formatter(state, exception),
                Properties = Properties(values),
                Specs = Specs(values),
                SourceFilePath = Value(values, "SourceFilePath"),
                SourceMemberName = Value(values, "SourceMemberName"),
                SourceLineNumber = IntValue(values, "SourceLineNumber"),
                Operation = Value(values, "Operation"),
                Result = Value(values, "Result"),
                DurationMilliseconds = LongValue(values, "DurationMilliseconds"),
                TraceId = traceId,
                SpanId = spanId,
                Segment = Value(values, "Segment"),
                Exception = Diagnostic(exception)
            });
        }
        catch
        {
            // Logging must never break the application.
        }
    }

    private static bool IsAccepted(string category)
        => category == "ToSic" || category.StartsWith("ToSic.", StringComparison.Ordinal)
           || category == "2sxc" || category.StartsWith("2sxc.", StringComparison.Ordinal);

    private static InsightsLogLevel Level(LogLevel level) => level switch
    {
        LogLevel.Trace => InsightsLogLevel.Trace,
        LogLevel.Debug => InsightsLogLevel.Debug,
        LogLevel.Information => InsightsLogLevel.Information,
        LogLevel.Warning => InsightsLogLevel.Warning,
        LogLevel.Error => InsightsLogLevel.Error,
        LogLevel.Critical => InsightsLogLevel.Critical,
        _ => InsightsLogLevel.Information
    };

    private static List<KeyValuePair<string, object?>> Values<TState>(TState state)
        => state is IEnumerable<KeyValuePair<string, object?>> pairs
            ? pairs.ToList()
            : [];

    private static ImmutableDictionary<string, string?> Properties(IEnumerable<KeyValuePair<string, object?>> values)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string?>();
        foreach (var pair in values)
            if (!builder.ContainsKey(pair.Key))
                builder[pair.Key] = Scalar(pair.Value);
        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, string?> Specs(IEnumerable<KeyValuePair<string, object?>> values)
    {
        // Specs stay separate because normal structured properties may legally use the same keys.
        if (values.FirstOrDefault(pair => pair.Key == "Specs").Value is not IEnumerable<KeyValuePair<string, string>> specs)
            return ImmutableDictionary<string, string?>.Empty;
        var builder = ImmutableDictionary.CreateBuilder<string, string?>();
        foreach (var pair in specs)
            builder[pair.Key] = pair.Value;
        return builder.ToImmutable();
    }

    private static string? Value(IEnumerable<KeyValuePair<string, object?>> values, string key)
        => Scalar(values.FirstOrDefault(pair => pair.Key == key).Value);

    private static int? IntValue(IEnumerable<KeyValuePair<string, object?>> values, string key)
        => values.FirstOrDefault(pair => pair.Key == key).Value is int value ? value : null;

    private static long? LongValue(IEnumerable<KeyValuePair<string, object?>> values, string key)
        => values.FirstOrDefault(pair => pair.Key == key).Value switch
        {
            long value => value,
            int value => value,
            _ => null
        };

    private static string? Scalar(object? value)
    {
        try { return value?.ToString(); }
        catch { return "<unavailable>"; }
    }

    private static InsightsExceptionDiagnostic? Diagnostic(Exception? exception)
        => exception == null
            ? null
            : new(exception.GetType().FullName ?? exception.GetType().Name, Scalar(exception.Message), Scalar(exception.StackTrace), Scalar(exception), ExceptionData(exception.Data), Diagnostic(exception.InnerException));

    private static ImmutableDictionary<string, string?> ExceptionData(System.Collections.IDictionary data)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string?>();
        try
        {
            foreach (System.Collections.DictionaryEntry entry in data)
                builder[Scalar(entry.Key) ?? "<null>"] = Scalar(entry.Value);
        }
        catch { }
        return builder.ToImmutable();
    }

    private sealed class InsightsLogger(InsightsLoggerProvider provider, string category) : ILogger
    {
        // Correlation comes from Activity; retaining arbitrary scope objects would break bounded storage.
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => IsAccepted(category) && logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => provider.Append(category, logLevel, eventId, state, exception, formatter);
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }
}
