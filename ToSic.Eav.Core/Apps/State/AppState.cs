using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A complete App state - usually cached in memory. <br/>
    /// Has many internal features for partial updates etc.
    /// But the primary purpose is to make sure the whole app is always available with everything. <br/>
    /// It also manages and caches relationships between entities of the same app.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public partial class AppState: AppBase
    {

        [PrivateApi("constructor, internal use only. should be internal, but ATM also used in FileAppStateLoader")]
        public AppState(ParentAppState parentApp, IAppIdentity app, string appGuidName, ILog parentLog): base($"App.St-{app.AppId}", new CodeRef())
        {
            ParentApp = parentApp;
            Log.Add($"AppState for App {app.AppId}");
            Log.Add($"Parent Inherits: Types: {parentApp.InheritContentTypes}, Entities: {parentApp.InheritEntities}");
            Init(app, new CodeRef(), parentLog);
            AppGuidName = appGuidName;
            
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

        [PrivateApi]
        public string AppGuidName { get; }

        /// <summary>
        /// The app-folder, which is pre-initialized very early on.
        /// Needed to pre-load file based content-types
        /// </summary>
        public string Folder
        {
            get => _folder;
            set
            {
                if (!Loading)
                    throw new Exception("Can't set AppState.Folder when not in loading state");
                _folder = value;
            }
        }
        private string _folder;


        /// <summary>
        /// The app-folder, which is pre-initialized very early on.
        /// Needed to pre-load file based content-types
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (!Loading) throw new Exception("Can't set AppState.Name when not in loading state");
                _name = value;
            }
        }
        private string _name;
    }
}