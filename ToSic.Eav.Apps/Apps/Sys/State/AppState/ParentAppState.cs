
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.Sys.Ancestors;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Apps.Sys.State;

/// <summary>
/// WIP v13 - should wrap a parent-app for re-use in a child-app
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ParentAppState(IAppStateCache? appState, bool inheritTypes, bool inheritEntities) : IParentAppState
{
    /// <inheritdoc />
#pragma warning disable CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
    public IAppStateCache? AppState { get; } = appState;
#pragma warning restore CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.

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
    [field: AllowNull, MaybeNull]
    public IEnumerable<IContentType> ContentTypes
        => field ??= GetInheritedTypes();

    /// <summary>
    /// The inherited entities
    /// </summary>
    public IEnumerable<IEntity> Entities
    {
        get
        {
            if (!InheritEntities || appState == null)
                return [];
            if (_entitiesCache != null)
                return _entitiesCache.List;
            _entitiesCache = new(appState,
                () => appState.List
                    .Select(WrapUnwrappedEntity)
                    .Where(e => e != null)
                    .Cast<IEntity>()
                    .ToImmutableOpt()
                );
            return _entitiesCache.List;
        }
    }
    private SynchronizedEntityList? _entitiesCache;


    public IContentType? GetContentType(string name)
        => InheritContentTypes && appState != null
            ? WrapUnwrappedContentType(appState.TryGetContentType(name))
            : null;

    private IEnumerable<IContentType> GetInheritedTypes()
    {
        if (!InheritContentTypes || appState == null)
            return new List<IContentType>(0);

        var types = appState.ContentTypes
            .Select(WrapUnwrappedContentType)
            .Where(ct => ct != null)
            .Cast<IContentType>();

        return SysPerfSettings.OptimizeParentApp
            ? types.ToListOpt()
            : types;
    }

    private IContentType? WrapUnwrappedContentType(IContentType? t)
    {
        if (t == null || t.HasAncestor() || appState == null)
            return t;
        return new ContentTypeWrapper(t, new Ancestor<IContentType>(new AppIdentity(appState), t.Id));
    }

    private IEntity? WrapUnwrappedEntity(IEntity? e)
    {
        if (e == null || e.HasAncestor() || appState == null)
            return e;
        return new EntityWrapper(e, new Ancestor<IEntity>(new AppIdentity(appState), e.EntityId));
    }

    // TODO:
    // - Entities
    // - Queries? 
}