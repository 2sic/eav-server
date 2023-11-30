using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data;

/// <summary>
/// Used to get relationships between entities.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal class RelationshipManager: IRelationshipManager
{
    // special note: ATM everything is an IEntity, so EntityLight is currently not supported

    /// <summary>
    /// This should be reworked, it often contains all relationships of the entire app
    /// </summary>
    private IEnumerable<EntityRelationship> AllRelationships { get; }


    /// <summary>
    /// Initializes a new instance of the RelationshipManager class.
    /// </summary>
    internal RelationshipManager(IEntityLight entity, IAppStateCache app, IEnumerable<EntityRelationship> fallbackRels = null)
    {
        _entity = entity;
        _appStateCache = app;
        _fallbackRels = fallbackRels;
        AllRelationships = app?.Relationships ?? fallbackRels ?? new List<EntityRelationship>();
    }
    private readonly IEntityLight _entity;
    private readonly IAppStateCache _appStateCache;
    private readonly IEnumerable<EntityRelationship> _fallbackRels;

    internal RelationshipManager(IEntityLight entity, RelationshipManager original)
        : this(entity, original?._appStateCache, original?._fallbackRels) { }

    /// <inheritdoc />
    public IEnumerable<IEntity> AllChildren => ChildRelationships().Select(r => r.Child);

    private IImmutableList<EntityRelationship> ChildRelationships()
    {
        if (_childRelationships != null) return _childRelationships.List;

        IImmutableList<EntityRelationship> GetChildrenInternal() 
            => AllRelationships.Where(r => r.Parent == _entity).ToImmutableList();

        if (_appStateCache == null) return GetChildrenInternal();
        _childRelationships = new SynchronizedList<EntityRelationship>(_appStateCache, GetChildrenInternal);
        return _childRelationships.List;
    }

    private SynchronizedList<EntityRelationship> _childRelationships;


    /// <inheritdoc />
    // note: don't cache the result, as it's already cache-chained
    public IEnumerable<IEntity> AllParents 
        => ParentRelationships().Select(r => r.Parent);
    // note: don't cache the result, as it's already cache-chained
    private IImmutableList<EntityRelationship> ParentRelationships()
    {
        if (_parentRelationships != null) return _parentRelationships.List;

        IImmutableList<EntityRelationship> GetParents() => AllRelationships.Where(r => r.Child == _entity).ToImmutableList();

        if (_appStateCache == null) return GetParents();
        _parentRelationships = new SynchronizedList<EntityRelationship>(_appStateCache, GetParents);
        return _parentRelationships.List;
    }
    private SynchronizedList<EntityRelationship> _parentRelationships;

    /// <inheritdoc />
    [PrivateApi]
    public IRelationshipChildren Children => _entity is IEntity entity ? new RelationshipChildren(entity.Attributes) : null ;



    #region Relationship-Navigation

    /// <inheritdoc />
    public List<IEntity> FindChildren(string field = null, string type = null, ILog log = null
    ) => log.Func($"field:{field}; type:{type}", () =>
    {
        List<IEntity> rels;
        if (string.IsNullOrEmpty(field))
            rels = ChildRelationships().Select(r => r.Child).ToList();
        else
        {
            // If the field doesn't exist, return empty list
            if (!((IEntity)_entity).Attributes.ContainsKey(field))
                return (new List<IEntity>(), "empty list, field doesn't exist");
                
            // if it does exist, still catch any situation where it's not a relationship field
            try
            {
                rels = Children[field].ToList();
            }
            catch
            {
                return (new List<IEntity>(), "empty list, doesn't seem to be relationship field");
            }
        }

        // Optionally filter by type
        if (!string.IsNullOrEmpty(type) && rels.Any())
            rels = rels.OfType(type).ToList();
        return (rels, rels.Count.ToString());
    });

    /// <inheritdoc />
    public List<IEntity> FindParents(string type = null, string field = null, ILog log = null
    ) => log.Func($"type:{type}; field:{field}", () =>
    {
        var list = ParentRelationships() as IEnumerable<EntityRelationship>;
        if (!string.IsNullOrEmpty(type))
            list = list.Where(r => r.Parent.Type.Is(type));

        if (string.IsNullOrEmpty(field))
        {
            var all = list.Select(r => r.Parent).ToList();
            return (all, all.Count.ToString());
        }
            
        list = list.Where(r => r.Parent.Relationships.FindChildren(field).Any(c => c == r.Child));
        var result = list.Select(r => r.Parent).ToList();
        return (result, result.Count.ToString());
    });

    #endregion
}