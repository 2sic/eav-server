using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public abstract class ReadBase<T>: HasLog where T: class
    {
        protected ReadBase(string logName) : base(logName)
        {
        }

        public T Init(AppState appState)
        {
            AppState = appState;
            return this as T;
        }

        public AppState AppState
        {
            get => _appState ?? throw new Exception("Can't use this Read class before setting AppState");
            protected set => _appState = value;
        }
        private AppState _appState;
    }
}
