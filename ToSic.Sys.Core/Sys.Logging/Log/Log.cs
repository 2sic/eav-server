using System.Runtime.CompilerServices;
using ToSic.Sys.Memory;

namespace ToSic.Sys.Logging;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class Log: ILog, ILogInternal, ICanEstimateSize
{
    /// <summary>
    /// Max logging depth, we should never attach loggers if we are past this level
    /// </summary>
    private const int MaxParentDepth = 100;

    #region Constructors

    /// <summary>
    /// Create a logger and optionally attach it to a parent logger
    /// </summary>
    /// <param name="name">name this logger should use</param>
    public Log(string name) : this(name, null, CodeRef.Create("", "", default)) { }

    /// <summary>
    /// Create a logger and optionally attach it to a parent logger
    /// </summary>
    /// <param name="name">name this logger should use</param>
    /// <param name="parent">optional parent logger to attach to</param>
    /// <param name="message">optional initial message to log</param>
    /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
    /// <param name="cName">auto pre filled by the compiler - the method name</param>
    /// <param name="cLine">auto pre filled by the compiler - the code line</param>
    public Log(string name,
        ILog? parent = default,
        string? message = default,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default)
        : this(name, parent, CodeRef.Create(cPath!, cName!, cLine), message) { }

    /// <summary>
    /// Create a logger and optionally attach it to a parent logger
    /// </summary>
    /// <param name="name">name this logger should use</param>
    /// <param name="parent">optional parent logger to attach to</param>
    /// <param name="code">The code reference - must be generated before</param>
    /// <param name="initialMessage">optional initial message to log</param>
    public Log(string name, ILog? parent, CodeRef code, string? initialMessage = default)
    {
        this.Rename(name);
        LinkTo(parent);
        if (initialMessage == default)
            return;
        this.AddInternal(initialMessage, code);
    }

    #endregion

    #region Name and Identifiers

    /// <summary>
    /// Unique ID of this logger, to not confuse it with other loggers.
    /// Is usually shown in the logs right after the name as [xy]
    /// </summary>
    internal string Id { get; } = Guid.NewGuid().ToString().Substring(0, 2);

    /// <summary>Stable storage identity; NameId remains a short display label.</summary>
    public string LogId { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// The topic this log belongs to, like Eav, Sxc, etc.
    /// Max length is 3 chars.
    /// </summary>
    internal string Scope = LogScopes.Unknown;

    /// <summary>
    /// The current logs name.
    /// Max length is 6 chars.
    /// </summary>
    internal string Name = LogConstants.NameUnknown;

    /// <summary>
    /// A standardized identifier of this log for showing in protocols
    /// </summary>
    public string NameId => $"{Scope}{(string.IsNullOrEmpty(Scope) ? "" : ".")}{Name}[{Id}]";


    public string FullIdentifier => (Parent as Log)?.FullIdentifier + NameId;

    #endregion

    #region Self Reference (WIP)

    private void AddEntry(Entry entry)
    {
        // prevent parallel threads from updating entries at the same time
        lock (Entries)
        {
            Entries.Add(entry);
        }
        (Parent as Log)?.AddEntry(entry);
    }

    Entry ILogInternal.CreateAndAdd(string? message, CodeRef? code, EntryOptions? options, Entry? parent,
        Microsoft.Extensions.Logging.LogLevel level, Exception? exception, bool publish)
    {
        var e = new Entry(this, message, WrapDepth, code, options)
        {
            ParentOperation = parent ?? CurrentOperation,
            Level = level,
        };
        AddEntry(e);
        if (publish && message != null)
            LogEventBridge.Write(e, exception);
        return e;
    }

    // Compatibility Fn/Done lifetimes are logger-owned, not request-wide ILogger scopes.
    // Explicit ILogCall parents also work when calls complete out of order.
    internal Entry? CurrentOperation
    {
        get
        {
            var current = _currentOperation.Value;
            while (current?.WrapOpenWasClosed == true)
                current = current.ParentOperation;
            return current ?? (Parent as Log)?.CurrentOperation;
        }
        set => _currentOperation.Value = value;
    }
    private readonly AsyncLocal<Entry?> _currentOperation = new();

    internal Entry? AttachmentOperation { get; private set; }

    internal Entry[] SnapshotEntries()
    {
        lock (Entries)
            return Entries.ToArray();
    }

    #endregion

    #region Properties

    public DateTime Created { get; } = DateTime.Now;

    /// <summary>
    /// How many inner wraps are currently open (when creating child entries using .Fn(...)
    /// </summary>
    internal int WrapDepth;

    /// <summary>
    /// Entries of this log and all children
    /// </summary>
    public List<Entry> Entries { get; } = [];

    /// <summary>
    /// Parent to which it's linked
    /// </summary>
    public ILog? Parent { get; private set; }

    /// <summary>
    /// How deep this logger is in the whole tree
    /// </summary>
    internal int Depth { get; set; }


    public bool Preserve
    {
        get;
        // ?? true;
        set
        {
            field = value;
            // pass it on to the parent if suddenly turned on, so that the chain knows if it should be preserved
            if (Parent is Log logParent)
                logParent.Preserve = value;
        }
    } = true;

    #endregion

    public SizeEstimate EstimateSize(ILog? log = default)
    {
        if (_estimate != null && Entries.Count == _entriesOnLastEstimate)
            return _estimate;
        _entriesOnLastEstimate = Entries.Count;
        var estimator = new MemorySizeEstimator(log);
        _estimate = estimator.EstimateMany([Id, Scope, Name, Created, Entries, WrapDepth, Preserve]);
        return _estimate;
    }
    private SizeEstimate? _estimate;
    private int _entriesOnLastEstimate = -1;
}
