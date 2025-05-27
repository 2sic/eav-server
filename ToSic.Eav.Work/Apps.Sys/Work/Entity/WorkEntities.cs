using System.Collections.Immutable;
using ToSic.Eav.DataSources;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.Internal.Work;

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
            ? [Scopes.Default, Scopes.SystemConfiguration]
            : new[] { Scopes.Default };
        return l.Return(AppWorkCtx.Data.List.Where(e => scopes.Contains(e.Type.Scope)));
    }

    /// <summary>
    /// Get this item or return null if not found
    /// </summary>
    public IEntity Get(int entityId) => AppWorkCtx.AppReader.List.FindRepoId(entityId);

    /// <summary>
    /// Get this item or return null if not found
    /// </summary>
    /// <returns></returns>
    public IEntity Get(Guid entityGuid) => AppWorkCtx.AppReader.List.One(entityGuid);


    public IEnumerable<IEntity> Get(string contentTypeName, IAppWorkCtxPlus overrideWorkCtx = default)
    {
        var typeFilter = dataSourceFactory.Value.Create<EntityTypeFilter>(attach: (overrideWorkCtx ?? AppWorkCtx).Data); // need to go to cache, to include published & unpublished
        typeFilter.TypeName = contentTypeName;
        return typeFilter.List;
    }


    public IEnumerable<IEntity> GetWithParentAppsExperimental(string contentTypeName)
    {
        var l = Log.Fn<IEnumerable<IEntity>>($"{nameof(contentTypeName)}: {contentTypeName}");
        var appWithParents = dataSourceFactory.Value.Create<AppWithParents>(attach: AppWorkCtx.Data);
        var newCtx = AppWorkCtx.SpawnNewWithPresetData(data: appWithParents);
        return l.Return(Get(contentTypeName, newCtx));
    }

}