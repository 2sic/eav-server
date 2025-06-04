using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Relationships.Sys;
using ToSic.Eav.StartUp;

namespace ToSic.Eav.Apps.State;

/// <summary>
/// A complete App state - usually cached in memory. <br/>
/// Has many internal features for partial updates etc.
/// But the primary purpose is to make sure the whole app is always available with everything. <br/>
/// It also manages and caches relationships between entities of the same app.
/// </summary>
[PrivateApi("this is just fyi - was marked as internal till v16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal partial class AppState: AppBase<MyServicesEmpty>, ILogShouldNeverConnect
{
    private static bool _loggedToBootLog = false;

    [PrivateApi("constructor, internal use only. should be internal, but ATM also used in FileAppStateLoader")]
    private AppState(ParentAppState parentApp, IAppIdentity id, string nameId, ILog parentLog): base(new(), $"App.St-{id.AppId}", connect: [])
    {
        // Track first time an app was built...
        if (!_loggedToBootLog)
        {
            BootLog.Log.Fn($"AppState for App {id.AppId}", timer: true).Done();
            // only stop once the first app is loaded; the master/root app doesn't have a parent
            if (id.AppId != KnownAppsConstants.PresetAppId)
                _loggedToBootLog = true;
        }

        var l = Log.Fn($"AppState for App {id.AppId}");
        this.LinkLog(parentLog, forceConnect: true);
        InitAppBaseIds(id);

        ParentApp = parentApp;
        l.A($"Parent Inherits: Types: {parentApp.InheritContentTypes}, Entities: {parentApp.InheritEntities}");
        CacheTimestampDelegate = CreateExpiryDelegate(parentApp, CacheTimestampPrivate);

        NameId = nameId;
            
        // Init the cache when it starts, because this number is needed in other places
        // Important: we must offset the first time stamp by 1 tick (1/100th nanosecond)
        // Because very small apps are loaded so quickly that otherwise it won't change the number after loading
        CacheResetTimestamp("init", offset: -1);  // do this very early, as this number is needed elsewhere

        Relationships = new(this);
        l.Done();
    }
    [PrivateApi]
    public IParentAppState ParentApp { get; }

    /// <summary>
    /// Manages all relationships between Entities
    /// </summary>
    public AppRelationshipManager Relationships { get; }

    /// <summary>
    /// WIP...
    /// </summary>
    IEnumerable<IEntityRelationship> IRelationshipSource.Relationships => Relationships;

    /// <summary>
    /// The official name identifier of the app, usually a Guid as a string, but often also "Default" for Content-Apps
    /// </summary>
    [PrivateApi]
    public string NameId { get; }

    /// <summary>
    /// The app-folder, which is pre-initialized very early on.
    /// Needed to pre-load file based content-types
    /// </summary>
    public string Folder
    {
        get;
        private set => field = ValueOrExceptionIfNotInLoadingState(value, nameof(Folder));
    }


    /// <summary>
    /// The app-folder, which is pre-initialized very early on.
    /// Needed to pre-load file based content-types
    /// </summary>
    public string Name
    {
        get;
        private set => field = ValueOrExceptionIfNotInLoadingState(value, nameof(Name));
    }

    private string ValueOrExceptionIfNotInLoadingState(string value, string property)
        => Loading ? value : throw new($"Can't set AppState.{property} when not in loading state");

    public bool IsHealthy { get; internal set; } = true;

    /// <summary>
    /// Health message containing errors or similar.
    /// </summary>
    /// <remarks>
    /// Initial value must be an empty string, since any errors will always be added to the previous messages.
    /// </remarks>
    public string HealthMessage { get; internal set; } = "";

}