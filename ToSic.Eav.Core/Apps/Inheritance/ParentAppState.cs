using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// WIP v13 - should wrap a parent-app for re-use in a child-app
    /// </summary>
    public class ParentAppState
    {
        /// <summary>
        /// The parent App
        /// </summary>
        public AppState AppState { get; }

        /// <summary>
        /// Determine if we should inherit ContentTypes or not
        /// </summary>
        public bool InheritContentTypes;

        /// <summary>
        /// The inherited content-types
        /// </summary>
        public IEnumerable<IContentType> ContentTypes => _contentTypes ?? (_contentTypes = GetInheritedTypes()); // AppState?.ContentTypes ?? new List<IContentType>(0));
        private IEnumerable<IContentType> _contentTypes;

        public ParentAppState(AppState appState, bool inheritTypes)
        {
            AppState = appState;
            InheritContentTypes = inheritTypes;
        }

        public IContentType GetContentType(string name) => InheritContentTypes ? WrapUnwrappedContentType(AppState.GetContentType(name)) : null;

        private IEnumerable<IContentType> GetInheritedTypes()
        {
            if (!InheritContentTypes || AppState == null) return new List<IContentType>(0);

            var types = AppState.ContentTypes.Select(WrapUnwrappedContentType);

            return types;
        }

        private IContentType WrapUnwrappedContentType(IContentType t)
        {
            if (t == null || t.HasAncestor()) return t;
            return new ContentTypeWrapper(t, new Ancestor<IContentType>(new AppIdentity(AppState), t.ContentTypeId));
        }

        // TODO:
        // - Entities
        // - Queries? 
    }
}
