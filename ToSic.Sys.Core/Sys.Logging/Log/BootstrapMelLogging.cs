using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>
/// Holds MEL boot events until the host's logger factory is available.
/// The host retains ownership of that factory.
/// </summary>
internal sealed class BootstrapMelLogging : ILoggerFactory
{
    private const int MaxEvents = 4096;
    private const int MaxCharacters = 1024 * 1024;
    private readonly object _sync = new();
    private readonly Queue<BufferedEvent> _pending = new();
    private ILoggerFactory? _host;
    private long _characters;
    private int _dropped;

    internal ILogFactory Factory { get; }

    internal BootstrapMelLogging() => Factory = new MelLogFactory(this);

    public ILogger CreateLogger(string categoryName) => new BootstrapLogger(this, categoryName);

    public void AddProvider(ILoggerProvider provider)
        => throw new NotSupportedException("Register providers with the host logger factory.");

    public void Dispose() { }

    internal int Bind(ILoggerFactory host)
    {
        lock (_sync)
        {
            if (_host != null)
            {
                if (!ReferenceEquals(_host, host))
                    throw new InvalidOperationException("The MEL bootstrap logger is already bound to another host factory.");
                return 0;
            }

            // Replay and new writes share this lock, so post-bind events cannot overtake boot events.
            _host = host;
            while (_pending.Count > 0)
            {
                try
                {
                    _pending.Dequeue().Forward(host);
                }
                catch
                {
                    // A faulty logging provider must not prevent application startup.
                    _dropped++;
                }
            }
            _characters = 0;
            return _dropped;
        }
    }

    private bool IsEnabled(BootstrapLogger logger, LogLevel level)
    {
        lock (_sync)
            return level != LogLevel.None && (_host == null || logger.RealLogger(_host).IsEnabled(level));
    }

    private void Write<TState>(BootstrapLogger logger, LogLevel level, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_sync)
        {
            if (_host != null)
            {
                logger.RealLogger(_host).Log(level, eventId, state, exception, formatter);
                return;
            }

            // Snapshot mutable MEL state now and retain the original timestamp without keeping live objects.
            var values = state is IEnumerable<KeyValuePair<string, object?>> pairs
                ? pairs.Select(pair => new KeyValuePair<string, object?>(pair.Key, Copy(pair.Value))).ToList()
                : [];
            var message = formatter(state, exception);
            if (values.Count == 0)
                values.Add(new("Message", message));
            values.Add(new("TimestampUtc", DateTime.UtcNow));
            var item = new BufferedEvent(logger, level, eventId, values.ToImmutableArray(), message,
                InsightsLoggerProvider.Diagnostic(exception));
            // An oversized event cannot fit; reject it before it evicts valid buffered events.
            if (item.Size > MaxCharacters)
            {
                _dropped++;
                return;
            }
            // Evict whole oldest events to keep pre-DI memory bounded without truncating accepted data.
            while (_pending.Count > 0 && (_pending.Count >= MaxEvents || _characters + item.Size > MaxCharacters))
            {
                _characters -= _pending.Dequeue().Size;
                _dropped++;
            }
            _pending.Enqueue(item);
            _characters += item.Size;
        }
    }

    private static object? Copy(object? value) => value switch
    {
        null => null,
        string text => text,
        bool or int or long or double or DateTime or TimeSpan => value,
        IEnumerable<KeyValuePair<string, string>> specs => specs.ToImmutableArray(),
        _ => SafeText(value)
    };

    private static string? SafeText(object? value)
    {
        try
        {
            return value?.ToString();
        }
        catch
        {
            return "<unavailable>";
        }
    }

    private sealed record BufferedEvent(BootstrapLogger Logger, LogLevel Level, EventId EventId,
        IReadOnlyList<KeyValuePair<string, object?>> Values, string? Message, InsightsExceptionDiagnostic? Diagnostic)
    {
        internal long Size => (Message?.Length ?? 0) + Values.Sum(pair => (long)pair.Key.Length + (pair.Value is IEnumerable<KeyValuePair<string, string>> specs
            ? specs.Sum(spec => (long)spec.Key.Length + (spec.Value?.Length ?? 0))
            : SafeText(pair.Value)?.Length ?? 0)) + DiagnosticSize(Diagnostic);

        internal void Forward(ILoggerFactory host)
            => Logger.RealLogger(host).Log(Level, EventId, Values,
                Diagnostic is null ? null : new DetachedException(Diagnostic), (_, _) => Message ?? "");
    }

    private static long DiagnosticSize(InsightsExceptionDiagnostic? diagnostic)
        => diagnostic is null ? 0
            : diagnostic.Type.Length + (diagnostic.Message?.Length ?? 0) + (diagnostic.StackTrace?.Length ?? 0)
              + (diagnostic.Details?.Length ?? 0)
              + diagnostic.Data.Sum(pair => (long)pair.Key.Length + (pair.Value?.Length ?? 0))
              + DiagnosticSize(diagnostic.InnerException);

    // Avoid retaining the live Exception before binding. Host providers receive this detached copy,
    // whose runtime type differs from the original type preserved in Diagnostic.
    internal sealed class DetachedException : Exception
    {
        internal InsightsExceptionDiagnostic Diagnostic { get; }

        internal DetachedException(InsightsExceptionDiagnostic diagnostic)
            : base(diagnostic.Message, diagnostic.InnerException is { } inner ? new DetachedException(inner) : null)
        {
            Diagnostic = diagnostic;
            foreach (var pair in diagnostic.Data)
                Data[pair.Key] = pair.Value;
        }

        public override string? StackTrace => Diagnostic.StackTrace;
        public override string ToString() => Diagnostic.Details ?? base.ToString();
    }

    private sealed class BootstrapLogger(BootstrapMelLogging owner, string category) : ILogger
    {
        private ILogger? _real;

        internal ILogger RealLogger(ILoggerFactory host) => _real ??= host.CreateLogger(category);

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => _real?.BeginScope(state) ?? NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => owner.IsEnabled(this, logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => owner.Write(this, logLevel, eventId, state, exception, formatter);
    }

    private sealed class NullScope : IDisposable
    {
        internal static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
