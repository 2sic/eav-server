using System.Collections.Immutable;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Sys.Caching.Synchronized;

namespace ToSic.Eav.Data.Relationships.Sys;

/// <summary>
/// Used to get relationships between entities.
/// </summary>
/// <remarks>
/// Note: ATM everything is an IEntity, so EntityLight is currently not supported
/// so even if the interface says IEntityLight, it will be converted to IEntity in most scenarios
///
/// Note #2: Do not change this to be a `record`. I tried it 2025-01-05, and it resulted in a stack overflow
/// which I wasn't able to track down, probably because of record specific equality checks.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class EntityRelationships(IEntity entity, IRelationshipSource app, IEnumerable<IEntityRelationship> fallbackRels = null)
    : IEntityRelationships
{
    private readonly IEntity _entity = entity as IEntity;
    private readonly IRelationshipSource _appSource = app;
    private readonly IEnumerable<IEntityRelationship> _fallbackRels = fallbackRels;

    /// <summary>
    /// Special constructor for cloning, where we attach the manager of the original
    /// </summary>
    internal static EntityRelationships ForClone(IEntity entity, EntityRelationships original)
        => new(entity, original?._appSource, original?._fallbackRels);

    /// <summary>
    /// This should be reworked, it often contains all relationships of the entire app
    /// </summary>
    private IEnumerable<IEntityRelationship> AllRelationships => field ??= _appSource?.Relationships ?? _fallbackRels ?? [];

    /// <inheritdoc />
    public IEnumerable<IEntity> AllChildren
        => ChildRelationships().Select(r => r.Child);

    private IImmutableList<IEntityRelationship> ChildRelationships()
    {
        // Most common scenario: already cached in synchronized list
        // Handled in Synchronized List - any changes will already be reflected
        if (_childRelationships != null)
            return _childRelationships.List;

        // If we don't have an AppStateCache, it's probably a temporary entity/relationship
        // rust return the result without caching
        if (_appSource == null)
            return GetChildrenUncached();

        // Standard scenario, just not cached yet - cache and return
        _childRelationships = new(_appSource, GetChildrenUncached);
        return _childRelationships.List;

    }

    private SynchronizedList<IEntityRelationship> _childRelationships;

    /// <summary>
    /// Directly retrieve the children - should only be called by the caching mechanism
    /// </summary>
    private IImmutableList<IEntityRelationship> GetChildrenUncached() =>
        AllRelationships
            .Where(r => ReferenceEquals(r.Parent, _entity))
            .ToImmutableOpt();


    /// <inheritdoc />
    // note: don't cache the result, as it's already cache-chained
    public IEnumerable<IEntity> AllParents 
        => ParentRelationships().Select(r => r.Parent);

    // note: don't cache the result, as it's already cache-chained
    private IImmutableList<IEntityRelationship> ParentRelationships()
    {
        // Most common scenario: already cached in synchronized list
        // Handled in Synchronized List - any changes will already be reflected
        if (_parentRelationships != null)
            return _parentRelationships.List;

        // If we don't have an AppStateCache, it's probably a temporary entity/relationship
        // rust return the result without caching
        if (_appSource == null)
            return GetParentsUncached();
        
        // Standard scenario, just not cached yet - cache and return
        _parentRelationships = new(_appSource, GetParentsUncached);
        return _parentRelationships.List;

    }
    private SynchronizedList<IEntityRelationship> _parentRelationships;

    #region Special Comparison

    /// <summary>
    /// Find all relationships, which have this one as child
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Should only be used inside <see cref="ParentRelationships"/>.
    /// But as a standalone function because of the extensive
    /// documentation necessary for the pub/hidden/draft scenario.
    /// </remarks>
    private IImmutableList<IEntityRelationship> GetParentsUncached()
    {
        var optimalCompare = SelfComparisonForParent();
        return AllRelationships
            .Where(r => optimalCompare(r.Child))
            .ToImmutableOpt();
    }


    /// <summary>
    /// For most publishing scenarios we can compare by reference.
    /// These two cases work, as there is only 1 entity in the cache, with the correct EntityId
    /// 1. entity is published
    /// 2. entity is draft
    ///
    /// But in the draft case it would fail. Because then we have 2 copies with different IDs.
    /// The admin would see the second draft item, but there is nothing pointing to it.
    /// Because of this, we need to compare by ID in that case.
    /// </summary>
    /// <returns></returns>
    private Func<IEntity, bool> SelfComparisonForParent()
    {
        var isCase3WithDraft = _entity is Entity realEntity && realEntity.EntityId != realEntity.RepositoryId;
        var myId = _entity.EntityId;
        return _selfComparisonForParent ??= isCase3WithDraft
            ? other => myId == other.EntityId
            : other => ReferenceEquals(_entity, other);
    }

    private Func<IEntity, bool> _selfComparisonForParent;

    #endregion


    /// <inheritdoc />
    public IRelationshipChildren Children => _entity is IEntity entity1
        ? new RelationshipChildren(entity1.Attributes)
        : null;



    #region Relationship-Navigation

    /// <inheritdoc />
    public List<IEntity> FindChildren(string field = null, string type = null, ILog log = null) 
    {
        var l = log.Fn<List<IEntity>>($"field:{field}; type:{type}");

        List<IEntity> rels;
        if (string.IsNullOrEmpty(field))
            rels = ChildRelationships().Select(r => r.Child).ToList();
        else
        {
            // If the field doesn't exist, return empty list
            if (!((IEntity)_entity).Attributes.ContainsKey(field))
                return l.Return([], "empty list, field doesn't exist");
                
            // if it does exist, still catch any situation where it's not a relationship field
            try
            {
                rels = Children[field].ToList();
            }
            catch
            {
                return l.Return([], "empty list, doesn't seem to be relationship field");
            }
        }

        // Optionally filter by type
        if (!string.IsNullOrEmpty(type) && rels.Any())
            rels = rels.OfType(type).ToList();

        return l.Return(rels, $"{rels.Count}");
    }

    /// <inheritdoc />
    public List<IEntity> FindParents(string type = null, string field = null, ILog log = null) 
    {
        var l = log.Fn<List<IEntity>>($"type:{type}; field:{field}");

        // Start with all parent relationships
        var allParents = ParentRelationships();

        // Optionally filter by type
        var afterType = type.HasValue()
            ? allParents.Where(r => r.Parent.Type.Is(type))
            : allParents;

        // Can we skip the field check?
        if (field.IsEmpty())
        {
            var parents = afterType
                .Select(r => r.Parent)
                .ToList();
            return l.Return(parents, $"{parents.Count}");
        }

        // Do the field check on the parent
        var relsWithFieldFilter = afterType
            .Where(r => r.Parent.Relationships
                .FindChildren(field)
                .Any(c => ReferenceEquals(c, r.Child))
            );

        // pick only the parents
        var parsAfterField = relsWithFieldFilter
            .Select(r => r.Parent)
            .ToList();
        return l.Return(parsAfterField, $"{parsAfterField.Count}");
    }

    #endregion

}