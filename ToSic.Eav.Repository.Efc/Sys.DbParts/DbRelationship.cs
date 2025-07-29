using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbRelationship(DbStorage.DbStorage db) : DbPartBase(db, "Db.Rels")
{
    internal void DoWhileQueueingRelationshipsUntracked(Action action)
    {
        var randomId = Guid.NewGuid().ToString().Substring(0, 4);
        var log = _isOutermostCall ? LogSummary : LogDetails;
        log.Do($"relationship queue:{randomId} start", () =>
        {
            // 1. check if it's the outermost call, in which case afterward we import
            var willPurgeQueue = _isOutermostCall;
            // 2. make sure any follow-up calls are not regarded as outermost
            _isOutermostCall = false;
            // 3. now run the inner code
            action.Invoke();
            // 4. now check if we were the outermost call, in if yes, save the data
            if (willPurgeQueue)
            {
                ImportRelationshipQueueAndSaveUntracked();
                _isOutermostCall = true; // reactivate, in case this is called again
            }
        });
    }

    private bool _isOutermostCall = true;

    private readonly List<RelationshipToSave> _saveQueue = [];



    /// <summary>
    /// Update Relationships of an Entity
    /// </summary>
    private void UpdateEntityRelationshipsAndSaveUntracked(ICollection<RelationshipUpdatePackage> packages)
    {
        var l = LogSummary.Fn($"Packages: {packages.Count}", timer: true);
        
        // Only log more details if details-logging is enabled
        if (LogDetails != null)
            foreach (var p in packages)
                l.A(l.Try(() => $"i:{p.EntityStubWithChildren.EntityId}, a:{p.AttributeId}, keys:[{string.Join(",", p.Targets)}]"));

        // remove existing Relationships that are not in new list
        var existingRelationships = packages
            .SelectMany(p => p.EntityStubWithChildren.RelationshipsWithThisAsParent
                .Where(e => e.AttributeId == p.AttributeId)
            )
            .ToList();

        // Delete all existing relationships (important, because the order, which is part of the key, is important afterward)
        // this is necessary after remove, because otherwise EF state tracking gets messed up
        if (existingRelationships.Count > 0)
            DbContext.DoAndSaveWithoutChangeDetection(
                () => DbContext.SqlDb.TsDynDataRelationships.RemoveRange(existingRelationships),
                $"found existing rels⋮{existingRelationships.Count} to flush");
        else
            l.A("No existing relationships to remove; skip.");

        // now save the changed relationships
        DbContext.DoAndSaveWithoutChangeDetection(() =>
            {
                foreach (var p in packages)
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
                }
            });

        l.Done("saved");
    }


    /// <summary>
    /// Import Entity Relationships Queue (Populated by UpdateEntityRelationships) and Clear Queue afterward.
    /// </summary>
    private void ImportRelationshipQueueAndSaveUntracked()
    {
        var lSum = LogSummary.Fn(timer: true);
        // if SaveOptions determines it, clear all existing relationships first
        var fullFlush = _saveQueue
            .Where(r => r.FlushAllEntityRelationships)
            .Select(r => r.ParentEntityId)
            .GroupBy(id => id)
            .Select(g => g.First())
            .ToList();

        DbContext.DoInTransaction(() =>
            {
                FlushChildrenRelationships(fullFlush);

                LogDetails.A($"will add relationships⋮{_saveQueue.Count}");

                var parentIds = _saveQueue
                    .Select(rel => rel.ParentEntityId)
                    .ToArray();
                if (parentIds.Any(p => p <= 0))
                    throw new("some parent has no id provided, can't update relationships");
                var parents = DbContext.Entities.GetDbEntitiesWithChildren(parentIds);
                LogDetails.A("Found parents to map:" + parents.Length);

                var allTargets = _saveQueue
                    .Where(rel => rel.ChildEntityGuids != null)
                    .SelectMany(rel => rel.ChildEntityGuids!
                        .Where(g => g.HasValue)
                        .Select(g => g!.Value)
                    )
                    .Distinct()
                    .ToArray();
                LogDetails.A("Total target IDs: " + allTargets.Length);
                var dbTargetIds = DbContext.Entities.GetMostCurrentDbEntities(allTargets);
                LogDetails.A("Total target entities (should match): " + dbTargetIds.Count);

                var updates = _saveQueue
                    .Select(relationship =>
                    {
                        var entityWithChildren = parents.Single(e => e.EntityId == relationship.ParentEntityId);

                        // start with the ID list - or if it doesn't exist, a new list
                        var childEntityIds = relationship.ChildEntityIds ?? [];

                        // if additional / alternative guids were specified, use those
                        if (childEntityIds.Count == 0 && relationship.ChildEntityGuids != null)
                            childEntityIds = relationship.ChildEntityGuids
                                .Select(childGuid => childGuid.HasValue
                                    ? dbTargetIds.TryGetValue(childGuid.Value, out var id)
                                        ? id
                                        : new int?()
                                    : new()
                                )
                                .ToListOpt();

                        return new RelationshipUpdatePackage
                        {
                            AttributeId = relationship.AttributeId,
                            EntityStubWithChildren = entityWithChildren,
                            Targets = childEntityIds
                        };
                    })
                    .ToListOpt();

                // Old before 2025-07-29; del ca. 2025-Q3
                //foreach (var relationship in _saveQueue)
                //{
                //    var entityWithChildren = parents.Single(e => e.EntityId == relationship.ParentEntityId);
                //    // start with the ID list - or if it doesn't exist, a new list
                //    var childEntityIds = relationship.ChildEntityIds ?? [];
                //    // if additional / alternative guids were specified, use those
                //    if (childEntityIds.Count == 0 && relationship.ChildEntityGuids != null)
                //        foreach (var childGuid in relationship.ChildEntityGuids)
                //            try
                //            {
                //                childEntityIds.Add(childGuid.HasValue
                //                    ? dbTargetIds.TryGetValue(childGuid.Value, out var id)
                //                        ? id
                //                        : new int?()
                //                    : new());
                //            }
                //            catch (InvalidOperationException)
                //            {
                //                // ignore, may occur if the child entity doesn't exist / wasn't created successfully
                //            }
                //    updates.Add(new()
                //    {
                //        AttributeId = relationship.AttributeId,
                //        EntityStubWithChildren = entityWithChildren,
                //        Targets = childEntityIds
                //    });
                //}

                UpdateEntityRelationshipsAndSaveUntracked(updates);
            }
        );

        _saveQueue.Clear();
        lSum.Done("done");
    }

    internal void FlushChildrenRelationships(ICollection<int> parentIds)
    {
        var l = LogSummary.Fn($"will do full-flush for {parentIds.Count} items", timer: true);
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

            foreach (var relationToDelete in ent.RelationshipsWithThisAsParent)
                DbContext.SqlDb.TsDynDataRelationships.Remove(relationToDelete);
        }

        // intermediate save (important) so that EF state tracking works
        DbContext.SqlDb.SaveChanges();
        l.Done();
    }

    internal void ChangeRelationships(IEntity eToSave, int entityId, List<TsDynDataAttribute> attributeDefs, SaveOptions so)
    {
        var l = LogDetails.Fn(timer: true);
        // some initial error checking
        if (entityId <= 0)
            throw new("can't work on relationships if entity doesn't have a repository id yet");

        // put all relationships into queue
        foreach (var attribute in eToSave.Attributes.Values)
        {
            // find attribute definition - will be null if the attribute cannot be found - in which case ignore
            var attribDef = attributeDefs.SingleOrDefault(a =>
                string.Equals(a.StaticName, attribute.Name, StringComparison.InvariantCultureIgnoreCase));
            if (attribDef is not { Type: nameof(ValueTypes.Entity) })
                continue;

            // check if there is anything at all (type doesn't matter yet)
            var valContents = attribute.Values.FirstOrDefault()?.ObjectContents;
            if (valContents == null)
                continue;
            
            valContents = valContents switch
            {
                IRelatedEntitiesValue entities => entities.Identifiers,
                Guid guid => new List<Guid> { guid },
                int i => new List<int> { i },
                _ => valContents
            };

            var guidList = (valContents as IEnumerable<Guid>)?.Select(p => (Guid?)p)
                           ?? (valContents as IEnumerable<Guid?>)?.Select(p => p);

            var entityIds = valContents as IEnumerable<int?>
                            ?? (valContents as IEnumerable<int>)?.Select(v => (int?)v);

            AddToQueue(attribDef.AttributeId, guidRels: guidList?.ToList(), intRels: entityIds?.ToList(), entityId, !so.PreserveUntouchedAttributes);
        }
        l.Done();
    }

    /// <summary>
    /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
    /// </summary>
    private void AddToQueue(int attributeId, List<Guid?>? guidRels, List<int?>? intRels, int entityId, bool flushAll)
    {
        var l = LogDetails.Fn($"id:{entityId}, guids⋮{guidRels?.Count}, ints⋮{intRels?.Count}");
        _saveQueue.Add(new()
        {
            AttributeId = attributeId,
            ChildEntityGuids = guidRels,
            ChildEntityIds = intRels,
            ParentEntityId = entityId,
            FlushAllEntityRelationships = flushAll
        });
        l.Done();
    }

}