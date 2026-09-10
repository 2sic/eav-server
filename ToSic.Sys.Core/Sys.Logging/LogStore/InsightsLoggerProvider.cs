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
    public const string OperationIdKey = "2sxc.OperationId";
    public const string CodeFileKey = "2sxc.Code.File";
    public const string CodeMemberKey = "2sxc.Code.Member";
    public const string CodeLineKey = "2sxc.Code.Line";

    /// <summary>
    /// Identity, code and trace keys must survive an exhausted property budget. Otherwise a noisy outer
    /// scope costs an inner event its bundle membership or its C# link.
    /// </summary>
    // ponytail: fixed reserve of 11 keys above MaxProperties; revisit only if this list grows.
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        LogExecution.LogIdKey, LogExecution.AmbientLogIdKey, LogExecution.AmbientOperationIdKey,
        LogExecution.InvocationLogIdKey, OperationIdKey,
        CodeFileKey, CodeMemberKey, CodeLineKey, "TraceId", "SpanId", "ParentSpanId",
    };

    /// <summary>Bounded scan so one oversized state cannot cost unbounded work per event.</summary>
    private const int MaxScannedPairs = InsightsLogStore.MaxProperties * 4;

    private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();
    internal int SegmentSize { get; set; } = LogConstants.LiveStoreSegmentSize;
    public ILogger CreateLogger(string categoryName) => new StoreLogger(this, categoryName);
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;
    public void Dispose() { }

    private void Write<TState>(string category, LogLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
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
        void Set(string key, string? value)
        {
            properties ??= ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
            if (properties.Count >= InsightsLogStore.MaxProperties && !properties.ContainsKey(key) && !Reserved.Contains(key))
            {
                truncated = true;
                return;
            }
            properties[Clip(key)!] = Clip(value)!;
        }
        void Capture(object? value)
        {
            if (value is not IEnumerable<KeyValuePair<string, object?>> pairs)
                return;
            var scanned = 0;
            foreach (var pair in pairs)
            {
                if (scanned++ >= MaxScannedPairs)
                {
                    truncated = true;
                    break;
                }
                // Never retain arbitrary scope objects or invoke their custom ToString implementations.
                if (pair.Value is string or bool or byte or short or int or long or float or double or decimal or Guid or DateTime)
                    Set(pair.Key, Convert.ToString(pair.Value, CultureInfo.InvariantCulture));
            }
        }
        // Replay/admission describes earlier work, not the request which happens to publish it.
        var captureContext = state is not LogEvent { Replay: true } && state is not LogEvent { Segment: not null };
        string? invocationLogId = null;
        if (captureContext)
            _scopes.ForEachScope((scope, _) =>
            {
                Capture(scope);
                // Without an execution boundary, the outermost invocation owns the bundle.
                if (invocationLogId == null && properties?.TryGetValue(LogExecution.InvocationLogIdKey, out var ownerId) == true)
                    invocationLogId = ownerId;
            }, 0);
        var ambientOperationId = properties?.TryGetValue(LogExecution.AmbientOperationIdKey, out var operationId) == true
            && long.TryParse(operationId, out var parsedOperationId) && parsedOperationId > 0
                ? parsedOperationId
                : (long?)null;
        var activity = captureContext ? Activity.Current : null;
        if (activity != null)
        {
            Set("TraceId", activity.TraceId.ToString());
            Set("SpanId", activity.SpanId.ToString());
            Set("ParentSpanId", activity.ParentSpanId.ToString());
        }
        LogEvent data;
        if (state is LogEvent bridge)
        {
            var ambientLogId = properties?.TryGetValue(LogExecution.AmbientLogIdKey, out var id) == true ? id : invocationLogId;
            foreach (var pair in bridge.Properties)
                Set(pair.Key, pair.Value);
            data = ambientLogId == null || ambientLogId == bridge.LogId || bridge.Ancestors.Contains(ambientLogId)
                ? bridge
                : bridge with { Ancestors = bridge.Ancestors.Add(ambientLogId) };
            if (data.WrapOpen && data.OperationId != ambientOperationId && !data.ParentOperationId.HasValue && ambientOperationId.HasValue)
                data = data with { ParentOperationId = ambientOperationId };
            else if (!data.WrapOpen && !data.OperationId.HasValue && ambientOperationId.HasValue)
                data = data with { OperationId = ambientOperationId };
        }
        else
        {
            Capture(state);
            // Native logs opt into an admitted bundle via a structured scope.
            var logId = properties?.TryGetValue(LogExecution.LogIdKey, out var scopedId) == true ? scopedId : invocationLogId;
            if (properties == null || logId == null)
                return;
            // The bridge carries its kind in LogEvent.Kind; only native callers need MEL event identity.
            if (eventId.Id != 0)
                Set("EventId", eventId.Id.ToString(CultureInfo.InvariantCulture));
            if (eventId.Name != null)
                Set("EventName", eventId.Name);
            data = new()
            {
                LogId = logId, Source = category, ShortSource = category, Created = DateTime.Now,
                Sequence = Entry.NextSequence(), Message = formatter(state, exception), Level = level,
                OperationId = properties.TryGetValue(OperationIdKey, out var operation) && long.TryParse(operation, out var id) ? id : ambientOperationId,
                Code = NativeCode(properties),
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
        var finalProperties = properties?.ToImmutable() ?? data.Properties;
        if (message != data.Message || result != data.Result || source != data.Source || shortSource != data.ShortSource
            || exceptionText != data.ExceptionText || code != data.Code || finalProperties != data.Properties)
            data = data with
            {
                Message = message, Result = result, Source = source, ShortSource = shortSource,
                ExceptionText = exceptionText, Code = code, Properties = finalProperties,
            };
        if (truncated)
            data = data with { Properties = data.Properties.SetItem(InsightsLogStore.TruncatedKey, "true") };
        store.Write(data, SegmentSize);
    }

    /// <summary>Projects caller information supplied as native properties into the Insights C# link.</summary>
    private static CodeRef? NativeCode(ImmutableDictionary<string, string>.Builder properties)
    {
        if (!properties.TryGetValue(CodeMemberKey, out var member) || string.IsNullOrEmpty(member))
            return null;
        properties.TryGetValue(CodeFileKey, out var path);
        properties.TryGetValue(CodeLineKey, out var line);
        return CodeRef.Create(path ?? "", member,
            int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : 0);
    }

    private sealed class StoreLogger(InsightsLoggerProvider provider, string category) : ILogger
    {
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => provider._scopes.Push(state);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
                provider.Write(category, logLevel, eventId, state, exception, formatter);
        }
    }
}
