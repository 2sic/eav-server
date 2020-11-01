﻿using System;
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
        internal AppState(IAppIdentity app, ILog parentLog): base($"App.St-{app.AppId}", new CodeRef())
        {
            Init(app, new CodeRef(), parentLog);
            CacheResetTimestamp();  // do this very early, as this number is needed elsewhere

	        Index = new Dictionary<int, IEntity>();
            Relationships = new AppRelationshipManager(this);
	        History.Add("app-state", Log);
        }


        /// <summary>
        /// Manages all relationships between Entities
        /// </summary>
        public AppRelationshipManager Relationships { get; }

        /// <summary>
        /// WIP - the app-path, which is pre-initialized very early on
        /// WIP 2020-05 for v11.x
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                if (!Loading)
                    throw new Exception("Can't set AppState.Path when not in loading state");
                _path = value;
            }
        }

        private string _path;
    }
}