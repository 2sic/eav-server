using System;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A complete App state - usually cached in memory. <br/>
    /// Has many internal features for partial updates etc.
    /// But the primary purpose is to make sure the whole app is always available with everything. <br/>
    /// It also manages and caches relationships between entities of the same app.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public partial class AppState: AppBase, ILogShouldNeverConnect
    {

        [PrivateApi("constructor, internal use only. should be internal, but ATM also used in FileAppStateLoader")]
        public AppState(ParentAppState parentApp, IAppIdentity id, string nameId, ILog parentLog): base($"App.St-{id.AppId}", new CodeRef())
        {
            Log.A($"AppState for App {id.AppId}");
            this.LinkLog(parentLog, null, forceConnect: true);
            Init(id);

            ParentApp = parentApp;
            Log.A($"Parent Inherits: Types: {parentApp.InheritContentTypes}, Entities: {parentApp.InheritEntities}");
            CacheExpiryDelegate = CreateExpiryDelegate(parentApp);

            NameId = nameId;
            
            // Init the cache when it starts, because this number is needed in other places
            // Important: we must offset the first time stamp by 1 tick (1/100th nanosecond)
            // Because very small apps are loaded so quickly that otherwise it won't change the number after loading
            CacheResetTimestamp("init", offset: -1);  // do this very early, as this number is needed elsewhere

            Relationships = new AppRelationshipManager(this);
        }
        [PrivateApi("WIP v13")]
        public readonly ParentAppState ParentApp;

        /// <summary>
        /// Manages all relationships between Entities
        /// </summary>
        public AppRelationshipManager Relationships { get; }

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
            get => _folder;
            set => _folder = ValueOrExceptionIfNotInLoadingState(value, nameof(Folder));
        }
        private string _folder;


        /// <summary>
        /// The app-folder, which is pre-initialized very early on.
        /// Needed to pre-load file based content-types
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = ValueOrExceptionIfNotInLoadingState(value, nameof(Name));
        }
        private string _name;

        private string ValueOrExceptionIfNotInLoadingState(string value, string property)
        {
            if (!Loading) throw new Exception($"Can't set AppState.{property} when not in loading state");
            return value;
        }
    }
}