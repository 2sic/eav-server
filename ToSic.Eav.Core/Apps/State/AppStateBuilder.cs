using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using ToSic.Eav.Apps.Reader;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.State
{
    public class AppStateBuilder: ServiceBase
    {
        public AppStateBuilder(): base("App.SttBld")
        {
        }

        public AppStateBuilder Init(AppState appState)
        {
            AppState = appState;
            Reader = new AppStateReader(AppState, Log);
            return this;
        }

        public AppStateBuilder InitForPreset()
        {
            AppState = new AppState(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, Log);
            Reader = new AppStateReader(AppState, Log);
            ForPreset = true;
            return this;
        }

        public bool ForPreset = false;
        public bool IsLoading = false;

        public AppState AppState
        {
            get => _appState ?? throw new Exception("Can't use before calling some init");
            private set => _appState = value;
        }
        private AppState _appState;

        public IAppStateInternal Reader
        {
            get => _reader ?? throw new Exception("Can't use before calling some init");
            private set => _reader = value;
        }
        private IAppStateInternal _reader;

        public void Load(string message, Action<AppState> loader)
        {
            var l = Log.Fn($"for a#{AppState.AppId}", message: message, timer: true);
            IsLoading = true;
            AppState.Load(() => loader(AppState));
            IsLoading = false;
            l.Done();
        }

        public void RemoveEntities(int[] repositoryIds, bool log)
        {
            AppState.Remove(repositoryIds, log);
        }

        public void InitMetadata()
        {
            AppState.InitMetadata();
        }

        public void SetNameAndFolder(string name, string folder)
        {
            AppState.Name = name;
            AppState.Folder = folder;
        }

        public void InitContentTypes(IList<IContentType> contentTypes)
        {
            AppState.InitContentTypes(contentTypes);
        }

        public void Add(IEntity newEntity, int? publishedId, bool log)
        {
            AppState.Add(newEntity, publishedId, log);
        }
    }
}
