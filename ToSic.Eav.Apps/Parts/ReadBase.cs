using System;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    public abstract class ReadBase : ServiceBase
    {
        protected ReadBase(string logName) : base(logName)
        {
        }

        public void Init(AppState appState) => AppState = appState;

        public AppState AppState
        {
            get => _appState ?? throw new Exception("Can't use this Read class before setting AppState");
            protected set => _appState = value;
        }
        private AppState _appState;
    }
}
