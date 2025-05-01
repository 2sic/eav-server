using System.Collections.Immutable;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;

namespace ToSic.Eav.Apps.State;

/// <summary>
/// WIP v13 - should wrap a parent-app for re-use in a child-app
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ParentAppState(IAppStateCache appState, bool inheritTypes, bool inheritEntities)
{
    /// <summary>
    /// The parent App
    /// </summary>
    public IAppStateCache AppState { get; } = appState;

    /// <summary>
    /// Determine if we should inherit ContentTypes or not
    /// </summary>
    public bool InheritContentTypes = inheritTypes;

    /// <summary>
    /// Set that entities / data should be inherited as well
    /// </summary>
    public bool InheritEntities = inheritEntities;

    /// <summary>
    /// The inherited content-types
    /// </summary>
    public IEnumerable<IContentType> ContentTypes => field ??= GetInheritedTypes();

    /// <summary>
    /// The inherited entities
    /// </summary>
    public IEnumerable<IEntity> Entities
    {
        get
        {
            if (!InheritEntities || AppState == null) return new List<IEntity>(0);
            if (_entitiesCache != null) return _entitiesCache.List;
            _entitiesCache = new(AppState,
                () => AppState.List.Select(WrapUnwrappedEntity).ToImmutableList());
            return _entitiesCache.List;
        }
    }
    private SynchronizedEntityList _entitiesCache;


    internal IContentType GetContentType(string name) => InheritContentTypes ? WrapUnwrappedContentType(((AppState)AppState).GetContentType(name)) : null;

    private IEnumerable<IContentType> GetInheritedTypes()
    {
        if (!InheritContentTypes || AppState == null) return new List<IContentType>(0);

        var types = ((AppState)AppState).ContentTypes.Select(WrapUnwrappedContentType);

        return types;
    }

    private IContentType WrapUnwrappedContentType(IContentType t)
    {
        if (t == null || t.HasAncestor()) return t;
        return new ContentTypeWrapper(t, new Ancestor<IContentType>(new AppIdentity(AppState), t.Id));
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