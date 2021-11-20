using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// WIP v13 - should wrap a parent-app for re-use in a child-app
    /// </summary>
    public class ParentAppState
    {
        public AppState AppState { get; }

        public bool InheritContentTypes;
        public IEnumerable<IContentType> ContentTypes => _contentTypes ?? (_contentTypes = AppState?.ContentTypes ?? new List<IContentType>(0));
        private IEnumerable<IContentType> _contentTypes;

        public ParentAppState(AppState appState, bool inheritTypes)
        {
            AppState = appState;
            InheritContentTypes = inheritTypes;
        }

        public IContentType GetContentType(string name) => InheritContentTypes ? AppState.GetContentType(name) : null;


        // TODO:
        // - Entities
        // - Queries? 
    }
}
