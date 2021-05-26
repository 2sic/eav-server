using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class EntityRuntime: PartOf<AppRuntime, EntityRuntime>
    {
        public EntityRuntime(): base ("RT.EntRun") { }

        #region Get

        /// <summary>
        /// All entities in the app - this also includes system entities like data-source configuration etc.
        /// </summary>
        public IImmutableList<IEntity> All => Parent.AppState.List;

        /// <summary>
        /// All content-entities. It does not include system-entity items.
        /// WARNING: ATM it respects published/unpublished because it's using the Data.
        /// It's not clear if this is actually intended.
        /// </summary>
        public IEnumerable<IEntity> OnlyContent(bool withConfiguration) =>
            Parent.Data.List.Where(e => (withConfiguration ? AppConstants.ScopesContentAndConfiguration : AppConstants.ScopesContent).Contains(e.Type.Scope));

        /// <summary>
        /// Get this item or return null if not found
        /// </summary>
        public IEntity Get(int entityId) => Parent.AppState.List.FindRepoId(entityId);

        /// <summary>
        /// Get this item or return null if not found
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        public IEntity Get(Guid entityGuid) => Parent.AppState.List.One(entityGuid);

        public IEnumerable<IEntity> Get(string contentTypeName)
        {
            var typeFilter = Parent.DataSourceFactory.GetDataSource<EntityTypeFilter>(Parent.Data); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.List;
        }

        #endregion


    }
}
