using System.Collections;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>Immutable bridge state without live log, service, or exception references.</summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record LogEvent : IEnumerable<KeyValuePair<string, object?>>
{
    public string Kind { get; init; } = "Entry";
    public string LogId { get; init; } = "";
    public ImmutableArray<string> Ancestors { get; init; } = [];
    public string? ParentLogId => Ancestors.FirstOrDefault();
    public string RootLogId => Ancestors.LastOrDefault() ?? LogId;
    public string Source { get; init; } = "";
    public string ShortSource { get; init; } = "";
    public string Scope { get; init; } = "";
    public string Name { get; init; } = "";
    public DateTime Created { get; init; }
    public DateTime? Completed { get; init; }
    public long Sequence { get; init; }
    public long? OperationId { get; init; }
    public long? ParentOperationId { get; init; }
    public int Depth { get; init; }
    public string? Message { get; init; }
    public string? Result { get; init; }
    public TimeSpan Elapsed { get; init; }
    public bool IsTimed { get; init; }
    public bool WrapOpen { get; init; }
    public bool WrapOpenWasClosed { get; init; }
    public bool HideCodeReference { get; init; }
    public bool ShowNewLines { get; init; }
    public CodeRef? Code { get; init; }
    public LogLevel Level { get; init; } = LogLevel.Trace;
    public string? ExceptionType { get; init; }
    public string? ExceptionText { get; init; }
    public string? Segment { get; init; }
    public ImmutableDictionary<string, string> Properties { get; init; }
        = ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase);
    public bool Replay { get; init; }

    internal static LogEvent ForLog(Log log)
    {
        var ancestors = ImmutableArray.CreateBuilder<string>();
        for (var parent = log.Parent as Log; parent != null; parent = parent.Parent as Log)
            ancestors.Add(parent.LogId);
        return new()
        {
            LogId = log.LogId, Ancestors = ancestors.ToImmutable(), Source = log.FullIdentifier,
            ShortSource = log.NameId, Scope = log.Scope, Name = log.Name, Created = log.Created,
        };
    }

    internal static LogEvent FromEntry(Entry entry, Exception? exception = null)
    {
        var log = entry.Owner!;
        var parentId = (entry.ParentOperation ?? log.AttachmentOperation)?.Sequence;
        return ForLog(log) with
        {
            Kind = entry.WrapOpenWasClosed ? "Completion" : entry.WrapOpen ? "Start" : "Entry",
            Sequence = entry.Sequence, Created = entry.Created, Completed = entry.Completed,
            OperationId = entry.WrapOpen ? entry.Sequence : parentId,
            ParentOperationId = entry.WrapOpen ? parentId : null,
            Message = entry.Message, Result = entry.Result, Depth = entry.Depth,
            WrapOpen = entry.WrapOpen, WrapOpenWasClosed = entry.WrapOpenWasClosed,
            Elapsed = entry.Elapsed, IsTimed = entry.IsTimed, Code = entry.Code, Level = entry.Level,
            HideCodeReference = entry.Options?.HideCodeReference == true,
            ShowNewLines = entry.Options?.ShowNewLines == true,
            ExceptionType = exception?.GetType().FullName, ExceptionText = exception?.ToString(),
        };
    }

    /// <summary>Standard ILogger structured state, also readable by other providers.</summary>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        yield return new("{OriginalFormat}", "{2sxc.Source} {2sxc.Message}");
        yield return new("2sxc.Kind", Kind);
        yield return new("2sxc.LogId", LogId);
        yield return new("2sxc.RootLogId", RootLogId);
        yield return new("2sxc.ParentLogId", ParentLogId);
        yield return new("2sxc.OperationId", OperationId);
        yield return new("2sxc.ParentOperationId", ParentOperationId);
        yield return new("2sxc.EntryId", Sequence);
        yield return new("2sxc.Scope", Scope);
        yield return new("2sxc.Name", Name);
        yield return new("2sxc.Source", Source);
        yield return new("2sxc.Depth", Depth);
        yield return new("2sxc.Message", Message);
        yield return new("2sxc.Result", Result);
        yield return new("2sxc.DurationMs", IsTimed ? Elapsed.TotalMilliseconds : null);
        yield return new("2sxc.Code.File", Code?.Path);
        yield return new("2sxc.Code.Member", Code?.Name);
        yield return new("2sxc.Code.Line", Code?.Line);
        yield return new("2sxc.Segment", Segment);
        foreach (var property in Properties)
            yield return new(property.Key.StartsWith("2sxc.", StringComparison.Ordinal)
                ? property.Key : "2sxc." + property.Key, property.Value);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => WrapOpenWasClosed
        ? $"{Source} completed {Message} in {Elapsed.TotalMilliseconds} ms: {Result}"
        : $"{Source} depth {Depth}: {Message}";
}
