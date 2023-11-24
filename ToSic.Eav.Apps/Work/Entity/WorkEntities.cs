using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Services;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Work;

/// <summary>
/// WIP - meant as a replacement of EntityRuntime with clean architecture
/// </summary>
public class WorkEntities: WorkUnitBase<IAppWorkCtxPlus>
{
    private readonly LazySvc<IDataSourcesService> _dataSourceFactory;
    public WorkEntities(LazySvc<IDataSourcesService> dataSourceFactory) : base("ApS.EnRead")
    {
        ConnectServices(
            _dataSourceFactory = dataSourceFactory
        );
    }

    /// <summary>
    /// All entities in the app - this also includes system entities like data-source configuration etc.
    /// </summary>
    public IImmutableList<IEntity> All() => AppWorkCtx.AppState.List;

    /// <summary>
    /// All content-entities. It does not include system-entity items.
    /// Important: it respects published/unpublished because it's using the Data.
    /// This is primarily meant for entity pickers
    /// </summary>
    public IEnumerable<IEntity> OnlyContent(bool withConfiguration)
    {
        var l = Log.Fn<IEnumerable<IEntity>>();
        var scopes = withConfiguration
            ? new[] { Scopes.Default, Scopes.SystemConfiguration }
            : new[] { Scopes.Default };
        return l.Return(AppWorkCtx.Data.List.Where(e => scopes.Contains(e.Type.Scope)));
    }

    /// <summary>
    /// Get this item or return null if not found
    /// </summary>
    public IEntity Get(int entityId) => AppWorkCtx.AppState.List.FindRepoId(entityId);

    /// <summary>
    /// Get this item or return null if not found
    /// </summary>
    /// <returns></returns>
    public IEntity Get(Guid entityGuid) => AppWorkCtx.AppState.List.One(entityGuid);


    public IEnumerable<IEntity> Get(string contentTypeName)
    {
        var typeFilter = _dataSourceFactory.Value.Create<EntityTypeFilter>(attach: AppWorkCtx.Data); // need to go to cache, to include published & unpublished
        typeFilter.TypeName = contentTypeName;
        return typeFilter.List;
    }


    public IEnumerable<IEntity> GetWithParentAppsExperimental(string contentTypeName)
    {
        var l = Log.Fn<IEnumerable<IEntity>>($"{nameof(contentTypeName)}: {contentTypeName}");
        var appWithParents = _dataSourceFactory.Value.Create<AppWithParents>(attach: AppWorkCtx.Data);
        var newCtx = AppWorkCtx.NewWithPresetData(data: appWithParents);
        return l.Return(Get(contentTypeName));
    }

}