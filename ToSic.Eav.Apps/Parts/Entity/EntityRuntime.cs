using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class EntityRuntime: PartOf<AppRuntime>
    {
        private readonly LazySvc<DataSourceFactory> _dataSourceFactory;
        public EntityRuntime(LazySvc<DataSourceFactory> dataSourceFactory): base ("RT.EntRun") =>
            ConnectServices(
                _dataSourceFactory = dataSourceFactory
            );

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
        public IEnumerable<IEntity> OnlyContent(bool withConfiguration) => Log.Func(() =>
        {
            var scopes = withConfiguration
                ? new[] { Scopes.Default, Scopes.SystemConfiguration }
                : new[] { Scopes.Default };
            return Parent.Data.List.Where(e => scopes.Contains(e.Type.Scope));
        });

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

        public IEnumerable<IEntity> Get(string contentTypeName
        // 2dm 2023-01-22 #maybeSupportIncludeParentApps
        // , bool includeParentApps = false
        )
        {
            var typeFilter = _dataSourceFactory.Value.Create<EntityTypeFilter>(source: Parent.Data); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            // 2dm 2023-01-22 #maybeSupportIncludeParentApps
            //if (includeParentApps) typeFilter.IncludeParentApps = true;
            return typeFilter.List;
        }
        public IEnumerable<IEntity> GetWithParentAppsExperimental(string contentTypeName) => Log.Func(() =>
        {
            var merged = _dataSourceFactory.Value.Create<AppWithParents>(source: Parent.Data);
            var typeFilter = _dataSourceFactory.Value.Create<EntityTypeFilter>(source: merged);
            typeFilter.TypeName = contentTypeName;
            return typeFilter.List;
        });

        #endregion


    }
}
