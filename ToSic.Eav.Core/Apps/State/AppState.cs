using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

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

        [PrivateApi("constructor, internal use only")]
        internal AppState(IAppIdentity app, string appGuidName, ILog parentLog): base($"App.St-{app.AppId}", new CodeRef())
        {
	        History.Add("app-state", Log);
            Log.Add($"AppState for App {app.AppId}");
            Init(app, new CodeRef(), parentLog);
            AppGuidName = appGuidName;
            CacheResetTimestamp();  // do this very early, as this number is needed elsewhere

	        Index = new Dictionary<int, IEntity>();
            Relationships = new AppRelationshipManager(this);
        }


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