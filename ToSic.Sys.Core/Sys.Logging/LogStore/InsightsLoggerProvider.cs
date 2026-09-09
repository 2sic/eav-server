using System.Collections.Immutable;
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
        var properties = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
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
                if (properties.Count >= InsightsLogStore.MaxProperties && !properties.ContainsKey(pair.Key))
                {
                    truncated = true;
                    break;
                }
                // Never retain arbitrary scope objects or invoke their custom ToString implementations.
                if (pair.Value is string or bool or byte or short or int or long or float or double or decimal or Guid or DateTime)
                    properties[Clip(pair.Key)!] = Clip(Convert.ToString(pair.Value, CultureInfo.InvariantCulture))!;
            }
        }
        _scopes.ForEachScope((scope, _) => Capture(scope), 0);
        LogEvent data;
        if (state is LogEvent bridge)
        {
            foreach (var pair in bridge.Properties.Take(InsightsLogStore.MaxProperties))
                properties[Clip(pair.Key)!] = Clip(pair.Value)!;
            data = bridge;
        }
        else
        {
            Capture(state);
            // Native logs opt into an admitted bundle via a structured scope.
            if (!properties.TryGetValue("2sxc.LogId", out var logId))
                return;
            data = new()
            {
                LogId = logId, Source = category, ShortSource = category, Created = DateTime.Now,
                Sequence = Entry.NextSequence(), Message = formatter(state, exception), Level = level,
                OperationId = properties.TryGetValue("2sxc.OperationId", out var operation) && long.TryParse(operation, out var id) ? id : null,
                ExceptionType = exception?.GetType().FullName, ExceptionText = exception?.ToString(),
            };
        }
        data = data with
        {
            Message = Clip(data.Message), Result = Clip(data.Result), Source = Clip(data.Source)!,
            ShortSource = Clip(data.ShortSource)!, ExceptionText = Clip(data.ExceptionText),
            Code = data.Code == null ? null : CodeRef.Create(Clip(data.Code.Path)!, Clip(data.Code.Name)!, data.Code.Line),
            Properties = properties.Take(InsightsLogStore.MaxProperties).ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
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
