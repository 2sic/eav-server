using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataSources;
using ToSic.Eav.Services;
using ToSic.Sys.OData;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// WIP - meant as a replacement of EntityRuntime with clean architecture
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntities(LazySvc<IDataSourcesService> dataSourceFactory)
    : WorkUnitBase<IAppWorkCtxPlus>("ApS.EnRead", connect: [dataSourceFactory])
{
    /// <summary>
    /// All entities in the app - this also includes system entities like data-source configuration etc.
    /// </summary>
    public IImmutableList<IEntity> All() => AppWorkCtx.AppReader.List;

    /// <summary>
    /// All content-entities. It does not include system-entity items.
    /// Important: it respects published/unpublished because it's using the Data.
    /// This is primarily meant for entity pickers
    /// </summary>
    public IEnumerable<IEntity> OnlyContent(bool withConfiguration)
    {
        var l = Log.Fn<IEnumerable<IEntity>>();
        var scopes = withConfiguration
            ? [ScopeConstants.Default, ScopeConstants.SystemConfiguration]
            : new[] { ScopeConstants.Default };
        return l.Return(AppWorkCtx.Data.List.Where(e => scopes.Contains(e.Type.Scope)));
    }

    /// <summary>
    /// Get this item or return null if not found
    /// </summary>
    public IEntity? Get(int entityId) => AppWorkCtx.AppReader.List.FindRepoId(entityId);

    /// <summary>
    /// Get this item or return null if not found
    /// </summary>
    /// <returns></returns>
    public IEntity? Get(Guid entityGuid) => AppWorkCtx.AppReader.List.GetOne(entityGuid);


    public IEnumerable<IEntity> Get(string contentTypeName, IAppWorkCtxPlus? overrideWorkCtx = default, Uri? fullRequest = null)
    {
        var dataSourcesService = dataSourceFactory.Value;
        var typeFilter = dataSourcesService.Create<EntityTypeFilter>(attach: (overrideWorkCtx ?? AppWorkCtx).Data); // need to go to cache, to include published & unpublished
        typeFilter.TypeName = contentTypeName;

        if (fullRequest is null)
            return typeFilter.List;

        var systemQueryOptions = SystemQueryOptionsParser.Parse(fullRequest);
        if (!systemQueryOptions.RawAllSystem.Any())
            return typeFilter.List;

        // v20 support OData filtering, sorting... if present in query string
        var query = UriQueryParser.Parse(systemQueryOptions);
        var engine = new ODataQueryEngine(dataSourcesService);
        var result = engine.Execute(typeFilter, query);
        return result.Items;
    }


    public IEnumerable<IEntity> GetWithParentAppsExperimental(string contentTypeName)
    {
        var l = Log.Fn<IEnumerable<IEntity>>($"{nameof(contentTypeName)}: {contentTypeName}");
        var appWithParents = dataSourceFactory.Value.Create<AppWithParents>(attach: AppWorkCtx.Data);
        var newCtx = AppWorkCtx.SpawnNewWithPresetData(data: appWithParents);
        return l.Return(Get(contentTypeName, newCtx));
    }

}