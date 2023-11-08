﻿using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Repository.Efc
{
    /// <summary>
    /// Wrapper for the EfcLoader, because we also need to do write operations on PrimaryApps, but the EFC loader cannot do that
    /// </summary>
    public class EfcRepositoryLoader: IRepositoryLoader
    {
        public EfcRepositoryLoader(DbDataController dataController) 
            => _dataController = dataController.Init(null, null);
        private readonly DbDataController _dataController;

        public ILog Log => _dataController.Loader.Log;

        public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source) => _dataController.Loader.ContentTypes(appId, source);

        public AppState AppStateRaw(int appId, CodeRef codeRef) => _dataController.Loader.AppStateRaw(appId, codeRef);

        public AppState AppStateInitialized(int appId, CodeRef codeRef) => _dataController.Loader.AppStateInitialized(appId, codeRef);

        public AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null) => _dataController.Loader.Update(app, startAt, entityIds);

        public IDictionary<int, Zone> Zones()
        {
            _dataController.Zone.AddMissingPrimaryApps();
            return _dataController.Loader.Zones();
        }

        public string PrimaryLanguage
        {
            get => _dataController.Loader.PrimaryLanguage;
            set => _dataController.Loader.PrimaryLanguage = value;
        }
    }
}
