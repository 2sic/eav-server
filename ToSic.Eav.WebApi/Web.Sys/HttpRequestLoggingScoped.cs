namespace ToSic.Eav.Web.Sys;

/// <summary>
/// WIP trying to move responsibility of creating a log for any request to a central place.
/// Goal is to make this available in other places where it may not be easy to connect to an existing log.
/// </summary>
public class HttpRequestLoggingScoped(ILogStore logStore) : ServiceWithSetup<HttpRequestLoggingScoped.Opts>("Web.HttpReqLog")
{
    /// <summary>
    /// Setup - may be called multiple times as we need a generator to get it, and it will always return the same (scoped) instance.
    /// </summary>
    /// <param name="options"></param>
    public override void Setup(Opts options)
    {
        if (_setupWasAlreadyCalled)
            return;
        _setupWasAlreadyCalled = true;

        base.Setup(options);

        RootLog = new Log(options.RootName);
        
        StoreEntry = logStore.Add(Options.Segment, RootLog);
    }

    private bool _setupWasAlreadyCalled;

    /// <summary>
    /// The root log, to add a message at the beginning of the request.
    /// </summary>
    public ILog RootLog { get; private set; } = null!;

    /// <summary>
    /// The log-store entry. Can be null if logging is disabled/paused.
    /// </summary>
    public LogStoreEntry? StoreEntry { get; private set; }

    /// <summary>
    /// Class to hold options for the HttpRequestLoggingService
    /// </summary>
    public record Opts
    {
        [field: AllowNull, MaybeNull]
        public string Segment
        {
            get => field ?? "unknown-log-segment";
            init;
        }

        [field: AllowNull, MaybeNull]
        public string RootName
        {
            get => field ?? "unknown";
            init;
        }
    }
}


