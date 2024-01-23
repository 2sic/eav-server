using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data;

/// <summary>
/// Used to get relationships between entities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the RelationshipManager class.
/// </remarks>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal class RelationshipManager(IEntityLight entity, IAppStateCache app, IEnumerable<EntityRelationship> fallbackRels = null)
    : IRelationshipManager
{
    // special note: ATM everything is an IEntity, so EntityLight is currently not supported
    // so even if the interface says IEntityLight, it will be converted to IEntity in most scenarios

    /// <summary>
    /// This should be reworked, it often contains all relationships of the entire app
    /// </summary>
    private IEnumerable<EntityRelationship> AllRelationships { get; } = app?.Relationships ?? fallbackRels ?? new List<EntityRelationship>();

    private readonly IAppStateCache _appStateCache = app;
    private readonly IEnumerable<EntityRelationship> _fallbackRels = fallbackRels;

    /// <summary>
    /// Special constructor for cloning, where we attach the manager of the original
    /// </summary>
    internal static RelationshipManager ForClone(IEntityLight entity, RelationshipManager original)
        => new(entity, original?._appStateCache, original?._fallbackRels);

    /// <inheritdoc />
    public IEnumerable<IEntity> AllChildren => ChildRelationships().Select(r => r.Child);

    private IImmutableList<EntityRelationship> ChildRelationships()
    {
        // Most common scenario: already cached in synchronized list
        // Handled in Synchronized List - any changes will already be reflected
        if (_childRelationships != null)
            return _childRelationships.List;

        // If we don't have an AppStateCache, it's probably a temporary entity/relationship
        // rust return the result without caching
        if (_appStateCache == null)
            return GetChildrenUncached();

        // Standard scenario, just not cached yet - cache and return
        _childRelationships = new(_appStateCache, GetChildrenUncached);
        return _childRelationships.List;

    }

    private SynchronizedList<EntityRelationship> _childRelationships;

    /// <summary>
    /// Directly retrieve the children - should only be called by the cachning mechanism
    /// </summary>
    private IImmutableList<EntityRelationship> GetChildrenUncached() =>
        AllRelationships
            .Where(r => ReferenceEquals(r.Parent, entity))
            .ToImmutableList();


    /// <inheritdoc />
    // note: don't cache the result, as it's already cache-chained
    public IEnumerable<IEntity> AllParents 
        => ParentRelationships().Select(r => r.Parent);

    // note: don't cache the result, as it's already cache-chained
    private IImmutableList<EntityRelationship> ParentRelationships()
    {
        // Most common scenario: already cached in synchronized list
        // Handled in Synchronized List - any changes will already be reflected
        if (_parentRelationships != null)
            return _parentRelationships.List;

        // If we don't have an AppStateCache, it's probably a temporary entity/relationship
        // rust return the result without caching
        if (_appStateCache == null)
            return GetParentsUncached();
        
        // Standard scenario, just not cached yet - cache and return
        _parentRelationships = new(_appStateCache, GetParentsUncached);
        return _parentRelationships.List;

    }
    private SynchronizedList<EntityRelationship> _parentRelationships;

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
    private IImmutableList<EntityRelationship> GetParentsUncached()
    {
        var optimalCompare = SelfComparisonForParent();
        return AllRelationships
            .Where(r => optimalCompare(r.Child))
            .ToImmutableList();
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
        var isCase3WithDraft = entity is Entity realEntity && realEntity.EntityId != realEntity.RepositoryId;
        var myId = entity.EntityId;
        return _selfComparisonForParent ??= isCase3WithDraft
            ? other => myId == other.EntityId
            : other => ReferenceEquals(entity, other);
    }

    private Func<IEntity, bool> _selfComparisonForParent;

    #endregion


    /// <inheritdoc />
    public IRelationshipChildren Children => entity is IEntity entity1
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
            if (!((IEntity)entity).Attributes.ContainsKey(field))
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


    #region Archive till #Remove2024-Q3

    // This section contains code created 2024-01-23 2dm
    // It was for fixing https://github.com/2sic/2sxc/issues/3258
    // After making changes to the AppStateRelationshipManager, it seems that this code is no longer necessary
    // But just in case I need it, I'll keep it here till 2024-Q3

    ///// <summary>
    ///// For most publishing scenarios we can compare by reference.
    ///// These two cases work, as there is only 1 entity in the cache, with the correct EntityId
    ///// 1. entity is published
    ///// 2. entity is draft
    /////
    ///// But in the draft case it would fail. Because then we have 2 copies with different IDs.
    ///// The admin would see the second draft item, but there is nothing pointing to it.
    ///// Because of this, we need to compare by ID in that case.
    ///// </summary>
    ///// <returns></returns>
    //private Func<IEntity, bool> SelfComparisonForChild()
    //{
    //    var realEntity = entity as Entity;
    //    var isCase3WithDraft = realEntity != null && realEntity.EntityId != realEntity.RepositoryId;
    //    var myId = realEntity?.RepositoryId ?? entity.EntityId;
    //    return _selfComparisonForChild ??= isCase3WithDraft
    //        ? other => myId == other.RepositoryId
    //        : other => ReferenceEquals(entity, other);
    //}

    //private Func<IEntity, bool> _selfComparisonForChild;

    #endregion
}