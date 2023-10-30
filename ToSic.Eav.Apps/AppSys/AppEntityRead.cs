using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Services;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.AppSys
{
    /// <summary>
    /// WIP - meant as a replacement of EntityRuntime with clean architecture
    /// </summary>
    public class AppEntityRead: ServiceBase
    {
        private readonly LazySvc<IDataSourcesService> _dataSourceFactory;
        public AppEntityRead(LazySvc<IDataSourcesService> dataSourceFactory) : base("ApS.EnRead")
        {
            ConnectServices(
                _dataSourceFactory = dataSourceFactory
            );
        }

        /// <summary>
        /// All entities in the app - this also includes system entities like data-source configuration etc.
        /// </summary>
        public IImmutableList<IEntity> All(IAppWorkCtx context) => context.AppState.List;

        /// <summary>
        /// All content-entities. It does not include system-entity items.
        /// WARNING: ATM it respects published/unpublished because it's using the Data.
        /// It's not clear if this is actually intended.
        /// </summary>
        public IEnumerable<IEntity> OnlyContent(IAppWorkCtx context, bool withConfiguration)
        {
            var l = Log.Fn<IEnumerable<IEntity>>();
            var scopes = withConfiguration
                ? new[] { Scopes.Default, Scopes.SystemConfiguration }
                : new[] { Scopes.Default };
            return l.Return(context.Data.List.Where(e => scopes.Contains(e.Type.Scope)));
        }

        /// <summary>
        /// Get this item or return null if not found
        /// </summary>
        public IEntity Get(IAppWorkCtx context, int entityId) => context.AppState.List.FindRepoId(entityId);

        /// <summary>
        /// Get this item or return null if not found
        /// </summary>
        /// <returns></returns>
        public IEntity Get(IAppWorkCtx context, Guid entityGuid) => context.AppState.List.One(entityGuid);


        public IEnumerable<IEntity> Get(IAppWorkCtx context, string contentTypeName)
        {
            var typeFilter = _dataSourceFactory.Value.Create<EntityTypeFilter>(attach: context.Data); // need to go to cache, to include published & unpublished
            typeFilter.TypeName = contentTypeName;
            return typeFilter.List;
        }


        public IEnumerable<IEntity> GetWithParentAppsExperimental(IAppWorkCtx context, string contentTypeName)
        {
            var l = Log.Fn<IEnumerable<IEntity>>($"{nameof(contentTypeName)}: {contentTypeName}");
            var appWithParents = _dataSourceFactory.Value.Create<AppWithParents>(attach: context.Data);
            var newCtx = new AppWorkCtx(context, data: appWithParents);
            return l.Return(Get(newCtx, contentTypeName));
        }

    }
}
