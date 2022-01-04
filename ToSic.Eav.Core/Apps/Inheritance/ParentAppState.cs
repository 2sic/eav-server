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
        public ParentAppState(AppState appState, bool inheritTypes, bool inheritEntities)
        {
            AppState = appState;
            InheritContentTypes = inheritTypes;
            InheritEntities = inheritEntities;
        }

        /// <summary>
        /// The parent App
        /// </summary>
        public AppState AppState { get; }

        /// <summary>
        /// Determine if we should inherit ContentTypes or not
        /// </summary>
        public bool InheritContentTypes;

        /// <summary>
        /// Set that entities / data should be inherited as well
        /// </summary>
        public bool InheritEntities;

        /// <summary>
        /// The inherited content-types
        /// </summary>
        public IEnumerable<IContentType> ContentTypes => _contentTypes ?? (_contentTypes = GetInheritedTypes());
        private IEnumerable<IContentType> _contentTypes;

        /// <summary>
        /// The inherited entities
        /// </summary>
        public IEnumerable<IEntity> Entities => _entities ?? (_entities = GetInheritedEntities());
        private IEnumerable<IEntity> _entities;



        private IEnumerable<IEntity> GetInheritedEntities()
        {
            if (!InheritEntities || AppState == null) return new List<IEntity>(0);
            
            var entities = AppState.List;
            // todo: maybe make it synced/attached?
            return entities.Select(WrapUnwrappedEntity);
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
        private IEntity WrapUnwrappedEntity(IEntity e)
        {
            if (e == null || e.HasAncestor()) return e;
            return new EntityWrapper(e, new Ancestor<IEntity>(new AppIdentity(AppState), e.EntityId));
        }

        // TODO:
        // - Entities
        // - Queries? 
    }
}
