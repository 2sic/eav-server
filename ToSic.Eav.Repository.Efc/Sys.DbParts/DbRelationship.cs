using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbRelationship(DbStorage.DbStorage db) : DbPartBase(db, "Db.Rels")
{

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
            DbStore.DoAndSaveWithoutChangeDetection(
                () => DbStore.SqlDb.TsDynDataRelationships.RemoveRange(existingRelationships),
                $"found existing rels⋮{existingRelationships.Count} to flush");
        else
            l.A("No existing relationships to remove; skip.");

        // now save the changed relationships
        DbStore.DoAndSaveWithoutChangeDetection(() =>
            {
                foreach (var p in packages)
                {
                    var newEntityIds = p.Targets.ToList();
                    // Create new relationships which didn't exist before
                    for (var i = 0; i < newEntityIds.Count; i++)
                        DbStore.SqlDb.TsDynDataRelationships.Add(new()
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
    internal void ImportRelationshipQueueAndSaveUntracked(ICollection<RelationshipToSave> list)
    {
        var lSum = LogSummary.Fn(timer: true);
        // if SaveOptions determines it, clear all existing relationships first
        var fullFlush = list
            .Where(r => r.FlushAllEntityRelationships)
            .Select(r => r.ParentEntityId)
            .GroupBy(id => id)
            .Select(g => g.First())
            .ToList();

        DbStore.DoInTransaction(() =>
            {
                FlushChildrenRelationships(fullFlush);

                LogDetails.A($"will add relationships⋮{list.Count}");

                var parentIds = list
                    .Select(rel => rel.ParentEntityId)
                    .ToArray();
                if (parentIds.Any(p => p <= 0))
                    throw new("some parent has no id provided, can't update relationships");
                var parents = DbStore.Entities.GetDbEntitiesWithChildren(parentIds);
                LogDetails.A("Found parents to map:" + parents.Length);

                var allTargets = list
                    .Where(rel => rel.ChildEntityGuids != null)
                    .SelectMany(rel => rel.ChildEntityGuids!
                        .Where(g => g.HasValue)
                        .Select(g => g!.Value)
                    )
                    .Distinct()
                    .ToArray();
                LogDetails.A("Total target IDs: " + allTargets.Length);
                var dbTargetIds = DbStore.Entities.GetMostCurrentDbEntities(allTargets);
                LogDetails.A("Total target entities (should match): " + dbTargetIds.Count);

                var updates = list
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

                UpdateEntityRelationshipsAndSaveUntracked(updates);


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
                //UpdateEntityRelationshipsAndSaveUntracked(updates);
            }
        );

        lSum.Done("done");
    }

    // IMPORTANT: 2dm made changes to this 2025-07-29 for ca. v20.00-04, but couldn't test it.
    // It probably works, but not verified, as it's almost never used, except probably when migrating non-json entities to json
    internal void FlushChildrenRelationships(ICollection<int> parentIds)
    {
        var l = LogSummary.Fn($"will do full-flush for {parentIds.Count} items", timer: true);

        // Delete all existing relationships - but not the target, just the relationship
        // note: can't use .Clear(), as that will try to actually delete the children
        if (parentIds.Count == 0)
        {
            l.Done("no parent IDs");
            return;
        }

        // intermediate save (important) so that EF state tracking works
        // ^^^ note: not sure if this is still relevant
        DbStore.DoAndSaveWithoutChangeDetection(() =>
        {
            var ent = DbStore.SqlDb.TsDynDataEntities
                .Include(e => e.RelationshipsWithThisAsParent)
                .Where(e => Enumerable.Contains(parentIds, e.EntityId))
                .SelectMany(e => e.RelationshipsWithThisAsParent)
                .ToListOpt();

            DbStore.SqlDb.TsDynDataRelationships.RemoveRange(ent);
        });
        l.Done();
    }


    // old before 2025-07-29; del ca. 2025-Q3
    //internal void FlushChildrenRelationships(ICollection<int> parentIds)
    //{
    //    var l = LogSummary.Fn($"will do full-flush for {parentIds.Count} items", timer: true);
    //    // Delete all existing relationships - but not the target, just the relationship
    //    // note: can't use .Clear(), as that will try to actually delete the children
    //    if (parentIds is not { Count: > 0 })
    //    {
    //        l.Done("no parent IDs");
    //        return;
    //    }

    //    foreach (var id in parentIds)
    //    {
    //        var ent = DbContext.SqlDb.TsDynDataEntities
    //            .Include(e => e.RelationshipsWithThisAsParent)
    //            .Single(e => e.EntityId == id);

    //        foreach (var relationToDelete in ent.RelationshipsWithThisAsParent)
    //            DbContext.SqlDb.TsDynDataRelationships.Remove(relationToDelete);
    //    }

    //    // intermediate save (important) so that EF state tracking works
    //    DbContext.SqlDb.SaveChanges();
    //    l.Done();
    //}

    internal void ChangeRelationships(IEntity eToSave, int entityId, List<TsDynDataAttribute> attributeDefs, SaveOptions so)
    {
        var relTasks = GetChangeRelationships(eToSave, entityId, attributeDefs, so);
        ImportRelationshipQueueAndSaveUntracked(relTasks);
    }

    internal ICollection<RelationshipToSave> GetChangeRelationships(IEntity eToSave, int entityId, List<TsDynDataAttribute> attributeDefs, SaveOptions so)
    {
        var l = LogDetails.Fn<ICollection<RelationshipToSave>>(timer: true);
        // some initial error checking
        if (entityId <= 0)
            throw new("can't work on relationships if entity doesn't have a repository id yet");

        // put all relationships into queue
        var relTasks = eToSave.Attributes.Values
            .Select(attribute =>
                {
                    // find attribute definition - will be null if the attribute cannot be found - in which case ignore
                    var attribDef = attributeDefs.SingleOrDefault(a =>
                        string.Equals(a.StaticName, attribute.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (attribDef is not { Type: nameof(ValueTypes.Entity) })
                        return null!;

                    // check if there is anything at all (type doesn't matter yet)
                    var valContents = attribute.Values.FirstOrDefault()?.ObjectContents;
                    if (valContents == null)
                        return null!;

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

                    var rel = PrepTask(attribDef.AttributeId, guidRels: guidList?.ToList(),
                        intRels: entityIds?.ToList(), entityId, !so.PreserveUntouchedAttributes);
                    return rel;
                }
            )
            .Where(t => t != null!)
            .ToListOpt();

        return l.Return(relTasks);
    }

    /// <summary>
    /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
    /// </summary>
    private RelationshipToSave PrepTask(int attributeId, List<Guid?>? guidRels, List<int?>? intRels, int entityId, bool flushAll)
    {
        var l = LogDetails.Fn<RelationshipToSave>($"id:{entityId}, guids⋮{guidRels?.Count}, ints⋮{intRels?.Count}");
        var rel = new RelationshipToSave
        {
            AttributeId = attributeId,
            ChildEntityGuids = guidRels,
            ChildEntityIds = intRels,
            ParentEntityId = entityId,
            FlushAllEntityRelationships = flushAll
        };
        return l.Return(rel);
    }

}