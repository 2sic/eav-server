using ToSic.Eav.Data.Relationships.Sys;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Parts;

internal class DbRelationship(DbDataController db) : DbPartBase(db, "Db.Rels")
{
    internal void DoWhileQueueingRelationships(Action action)
    {
        var randomId = Guid.NewGuid().ToString().Substring(0, 4);
        Log.Do($"relationship queue:{randomId} start", () =>
        {
            // 1. check if it's the outermost call, in which case afterwards we import
            var willPurgeQueue = _isOutermostCall;
            // 2. make sure any follow-up calls are not regarded as outermost
            _isOutermostCall = false;
            // 3. now run the inner code
            action.Invoke();
            // 4. now check if we were the outermost call, in if yes, save the data
            if (willPurgeQueue)
            {
                ImportRelationshipQueueAndSave();
                _isOutermostCall = true; // reactivate, in case this is called again
            }
        });
    }

    private bool _isOutermostCall = true;

    private readonly List<RelationshipToSave> _saveQueue = [];



    /// <summary>
    /// Update Relationships of an Entity
    /// </summary>
    private void UpdateEntityRelationshipsAndSave(List<RelationshipUpdatePackage> packages)
    {
        var l = Log.Fn(timer: true);
        packages.ForEach(p => l.A(l.Try(() => $"i:{p.EntityStubWithChildren.EntityId}, a:{p.AttributeId}, keys:[{string.Join(",", p.Targets)}]")));
        // remove existing Relationships that are not in new list
        var existingRelationships = packages
            .SelectMany(p => p.EntityStubWithChildren.RelationshipsWithThisAsParent
                .Where(e => e.AttributeId == p.AttributeId)
            )
            .ToList();

        // Delete all existing relationships (important, because the order, which is part of the key, is important afterward)
        if (existingRelationships.Count > 0)
        {
            l.A($"found existing rels⋮{existingRelationships.Count}");
            // this is necessary after remove, because otherwise EF state tracking gets messed up
            DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.TsDynDataRelationships.RemoveRange(existingRelationships));
        }

        // now save the changed relationships
        DbContext.DoAndSaveWithoutChangeDetection(() =>
            packages.ForEach(p =>
            {
                var newEntityIds = p.Targets.ToList();
                // Create new relationships which didn't exist before
                for (var i = 0; i < newEntityIds.Count; i++)
                    DbContext.SqlDb.TsDynDataRelationships.Add(new()
                    {
                        AttributeId = p.AttributeId,
                        ChildEntityId = newEntityIds[i],
                        SortOrder = i,
                        ParentEntityId = p.EntityStubWithChildren.EntityId
                    });
            }));

        l.Done();
    }

    /// <summary>
    /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
    /// </summary>
    private void AddToQueue(int attributeId, List<Guid?> newValue, int entityId, bool flushAll)
    {
        Log.A($"add to queue for i:{entityId}, guids⋮{newValue.Count}");
        _saveQueue.Add(new()
        {
            AttributeId = attributeId,
            ChildEntityGuids = newValue,
            ParentEntityId = entityId,
            FlushAllEntityRelationships = flushAll
        });
    }

    /// <summary>
    /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
    /// </summary>
    private void AddToQueue(int attributeId, List<int?> newValue, int entityId, bool flushAll)
    {
        Log.A($"add to int for i:{entityId}, ints⋮{newValue.Count}");
        _saveQueue.Add(new()
        {
            AttributeId = attributeId,
            ChildEntityIds = newValue,
            ParentEntityId = entityId,
            FlushAllEntityRelationships = flushAll
        });
    }

    /// <summary>
    /// Import Entity Relationships Queue (Populated by UpdateEntityRelationships) and Clear Queue afterward.
    /// </summary>
    private void ImportRelationshipQueueAndSave()
    {
        var l = Log.Fn(timer: true);
        // if SaveOptions determines it, clear all existing relationships first
        var fullFlush = _saveQueue
            .Where(r => r.FlushAllEntityRelationships)
            .Select(r => r.ParentEntityId)
            .GroupBy(id => id)
            .Select(g => g.First())
            .ToList<int>();

        DbContext.DoInTransaction(() =>
            {
                FlushChildrenRelationships(fullFlush);

                l.A($"will add relationships⋮{_saveQueue.Count}");

                var parentIds = _saveQueue
                    .Select(rel => rel.ParentEntityId)
                    .ToArray();
                if (parentIds.Any(p => p <= 0))
                    throw new("some parent has no id provided, can't update relationships");
                var parents = DbContext.Entities.GetDbEntitiesWithChildren(parentIds);
                l.A("Found parents to map:" + parents.Length);

                var allTargets = _saveQueue
                    .Where(rel => rel.ChildEntityGuids != null)
                    .SelectMany(rel => rel.ChildEntityGuids
                        .Where(g => g.HasValue)
                        .Select(g => g.Value)
                    )
                    .Distinct()
                    .ToArray();
                l.A("Total target IDs: " + allTargets.Length);
                var dbTargetIds = DbContext.Entities.GetMostCurrentDbEntities(allTargets);
                l.A("Total target entities (should match): " + dbTargetIds.Count);

                var updates = new List<RelationshipUpdatePackage>();
                foreach (var relationship in _saveQueue)
                {
                    var entityWithChildren = parents.Single(e => e.EntityId == relationship.ParentEntityId);

                    // start with the ID list - or if it doesn't exist, a new list
                    var childEntityIds = relationship.ChildEntityIds ?? [];

                    // if additional / alternative guids were specified, use those
                    if (childEntityIds.Count == 0 && relationship.ChildEntityGuids != null)
                        foreach (var childGuid in relationship.ChildEntityGuids)
                            try
                            {
                                childEntityIds.Add(childGuid.HasValue
                                    ? dbTargetIds.TryGetValue(childGuid.Value, out var id) ? id : new int?()
                                    : new());
                            }
                            catch (InvalidOperationException)
                            {
                                // ignore, may occur if the child entity doesn't exist / wasn't created successfully
                            }

                    updates.Add(new(entityWithChildren, relationship.AttributeId, childEntityIds));
                }

                UpdateEntityRelationshipsAndSave(updates);
            }
        );

        _saveQueue.Clear();
        l.Done("done");
    }

    private class RelationshipUpdatePackage(TsDynDataEntity entityStubWithChildren, int attributeId, List<int?> relationships)
    {
        public readonly int AttributeId = attributeId;
        public readonly List<int?> Targets = relationships;
        /// <summary>
        /// This is just a stub with EntityId, but also MUST have the `RelationshipsWithThisAsParent` filled
        /// If future code needs it to be filled more, make sure it's constructed that way before.
        /// </summary>
        public readonly TsDynDataEntity EntityStubWithChildren = entityStubWithChildren;
    }

    internal void FlushChildrenRelationships(List<int> parentIds)
    {
        var l = Log.Fn($"will do full-flush for {parentIds?.Count} items", timer: true);
        // Delete all existing relationships - but not the target, just the relationship
        // note: can't use .Clear(), as that will try to actually delete the children
        if (parentIds is not { Count: > 0 })
        {
            l.Done("no parent IDs");
            return;
        }

        foreach (var id in parentIds)
        {
            var ent = DbContext.SqlDb.TsDynDataEntities
                .Include(e => e.RelationshipsWithThisAsParent)
                .Single(e => e.EntityId == id);

            //var ent = DbContext.Entities.GetDbEntity(id);
            foreach (var relationToDelete in ent.RelationshipsWithThisAsParent)
                DbContext.SqlDb.TsDynDataRelationships.Remove(relationToDelete);
        }

        // intermediate save (important) so that EF state tracking works
        DbContext.SqlDb.SaveChanges();
        l.Done();
    }


    #region Internal Helper Classes

    private struct RelationshipToSave
    {
        public int AttributeId { get; set; }
        public List<Guid?> ChildEntityGuids { get; set; }
        public List<int?> ChildEntityIds { get; set; }

        public int ParentEntityId { get; set; }


        public bool FlushAllEntityRelationships { get; set; }
    }

    #endregion

    internal void ChangeRelationships(IEntity eToSave, TsDynDataEntity dbEntity, List<TsDynDataAttribute> attributeDefs, SaveOptions so)
    {
        var l = Log.Fn(timer: true);
        // some initial error checking
        if (dbEntity.EntityId <= 0)
            throw new("can't work on relationships if entity doesn't have a repository id yet");

        DoWhileQueueingRelationships(() =>
        {
            // put all relationships into queue
            foreach (var attribute in eToSave.Attributes.Values)
            {
                // find attribute definition - will be null if the attribute cannot be found - in which case ignore
                var attribDef = attributeDefs.SingleOrDefault(a =>
                    string.Equals(a.StaticName, attribute.Name, StringComparison.InvariantCultureIgnoreCase));
                if (attribDef == null || attribDef.Type != ValueTypes.Entity.ToString()) continue;

                // check if there is anything at all (type doesn't matter yet)
                var valContents = attribute.Values?.FirstOrDefault()?.ObjectContents;
                switch (valContents)
                {
                    case null:
                        continue;
                    case IRelatedEntitiesValue entities:
                        valContents = entities.Identifiers;
                        break;
                    case Guid guid:
                        valContents = new List<Guid> { guid };
                        break;
                    case int i:
                        valContents = new List<int> { i };
                        break;
                }

                if (valContents is List<Guid> or List<Guid?>)
                {
                    var guidList = (valContents as List<Guid>)?.Select(p => (Guid?)p) ??
                                   ((List<Guid?>)valContents).Select(p => p);
                    AddToQueue(attribDef.AttributeId, guidList.ToList(), dbEntity.EntityId,
                        !so.PreserveUntouchedAttributes);
                }
                else if (valContents is List<int> or List<int?>)
                {
                    var entityIds = valContents as List<int?> ?? ((List<int>)valContents).Select(v => (int?)v).ToList();
                    AddToQueue(attribDef.AttributeId, entityIds, dbEntity.EntityId,
                        !so.PreserveUntouchedAttributes);
                }
            }
        });
        l.Done();
    }
}