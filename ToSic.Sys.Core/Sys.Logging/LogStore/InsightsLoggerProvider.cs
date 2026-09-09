using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>Captures bridge events and structured ILogger scopes in the same application process.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
[ProviderAlias("2sxcInsights")]
public sealed class InsightsLoggerProvider(InsightsLogStore store) : ILoggerProvider, ISupportExternalScope
{
    private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();
    internal int SegmentSize { get; set; } = LogConstants.LiveStoreSegmentSize;
    public ILogger CreateLogger(string categoryName) => new StoreLogger(this, categoryName);
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;
    public void Dispose() { }

    private void Write<TState>(string category, LogLevel level, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!store.Enabled)
            return;
        ImmutableDictionary<string, string>.Builder? properties = null;
        var truncated = false;
        string? Clip(string? value)
        {
            if (value == null || value.Length <= InsightsLogStore.MaxTextLength)
                return value;
            truncated = true;
            return value.Substring(0, InsightsLogStore.MaxTextLength - 1) + "…";
        }
        void Capture(object? value)
        {
            if (value is not IEnumerable<KeyValuePair<string, object?>> pairs)
                return;
            foreach (var pair in pairs.Take(InsightsLogStore.MaxProperties))
            {
                if (properties?.Count >= InsightsLogStore.MaxProperties && !properties.ContainsKey(pair.Key))
                {
                    truncated = true;
                    break;
                }
                // Never retain arbitrary scope objects or invoke their custom ToString implementations.
                if (pair.Value is string or bool or byte or short or int or long or float or double or decimal or Guid or DateTime)
                    (properties ??= ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase))
                        [Clip(pair.Key)!] = Clip(Convert.ToString(pair.Value, CultureInfo.InvariantCulture))!;
            }
        }
        // Replay/admission describes earlier work, not the request which happens to publish it.
        var captureContext = state is not LogEvent { Replay: true } && state is not LogEvent { Segment: not null };
        if (captureContext)
            _scopes.ForEachScope((scope, _) => Capture(scope), 0);
        var activity = captureContext ? Activity.Current : null;
        if (activity != null)
        {
            properties ??= ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
            properties["TraceId"] = activity.TraceId.ToString();
            properties["SpanId"] = activity.SpanId.ToString();
            properties["ParentSpanId"] = activity.ParentSpanId.ToString();
        }
        LogEvent data;
        if (state is LogEvent bridge)
        {
            var ambientLogId = properties?.TryGetValue(LogExecution.AmbientLogIdKey, out var id) == true ? id : null;
            foreach (var pair in bridge.Properties.Take(InsightsLogStore.MaxProperties))
                (properties ??= ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase))
                    [Clip(pair.Key)!] = Clip(pair.Value)!;
            data = ambientLogId == null || ambientLogId == bridge.LogId || bridge.Ancestors.Contains(ambientLogId)
                ? bridge
                : bridge with { Ancestors = bridge.Ancestors.Add(ambientLogId) };
        }
        else
        {
            Capture(state);
            // Native logs opt into an admitted bundle via a structured scope.
            if (properties?.TryGetValue(LogExecution.LogIdKey, out var logId) != true || logId == null)
                return;
            data = new()
            {
                LogId = logId, Source = category, ShortSource = category, Created = DateTime.Now,
                Sequence = Entry.NextSequence(), Message = formatter(state, exception), Level = level,
                OperationId = properties.TryGetValue("2sxc.OperationId", out var operation) && long.TryParse(operation, out var id) ? id : null,
                ExceptionType = exception?.GetType().FullName, ExceptionText = exception?.ToString(),
            };
        }
        var message = Clip(data.Message);
        var result = Clip(data.Result);
        var source = Clip(data.Source)!;
        var shortSource = Clip(data.ShortSource)!;
        var exceptionText = Clip(data.ExceptionText);
        var code = data.Code;
        if (code != null)
        {
            var path = Clip(code.Path)!;
            var name = Clip(code.Name)!;
            if (path != code.Path || name != code.Name)
                code = CodeRef.Create(path, name, code.Line);
        }
        var finalProperties = properties == null
            ? data.Properties
            : properties.Take(InsightsLogStore.MaxProperties).ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        if (message != data.Message || result != data.Result || source != data.Source || shortSource != data.ShortSource
            || exceptionText != data.ExceptionText || code != data.Code || finalProperties != data.Properties)
            data = data with
            {
                Message = message, Result = result, Source = source, ShortSource = shortSource,
                ExceptionText = exceptionText, Code = code, Properties = finalProperties,
            };
        if (truncated)
            data = data with { Properties = data.Properties.SetItem("2sxc.Truncated", "true") };
        store.Write(data, SegmentSize);
    }

    private sealed class StoreLogger(InsightsLoggerProvider provider, string category) : ILogger
    {
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => provider._scopes.Push(state);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
                provider.Write(category, logLevel, state, exception, formatter);
        }
    }
}
