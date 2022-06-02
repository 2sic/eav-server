using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbRelationship: BllCommandBase
    {
        public DbRelationship(DbDataController db) : base(db, "Db.Rels") {}

        internal void DoWhileQueueingRelationships(Action action)
        {
            var randomId = Guid.NewGuid().ToString().Substring(0, 4);
            var wrapLog = Log.Fn($"relationship queue:{randomId} start");

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

            wrapLog.Done("completed");
        }

        private bool _isOutermostCall = true;

        private readonly List<RelationshipToSave> _saveQueue = new List<RelationshipToSave>();



        /// <summary>
        /// Update Relationships of an Entity
        /// </summary>
        private void UpdateEntityRelationshipsAndSave(List<RelationshipUpdatePackage> packages)
        {
            var wrapLog = Log.Fn(startTimer: true);
            packages.ForEach(p => Log.A(() => $"i:{p.Entity.EntityId}, a:{p.AttributeId}, keys:[{string.Join(",", p.Targets)}]"));
            // remove existing Relationships that are not in new list
            var existingRelationships = packages.SelectMany(p => p.Entity.RelationshipsWithThisAsParent
                .Where(e => e.AttributeId == p.AttributeId))
                .ToList();

            // Delete all existing relationships (important, because the order, which is part of the key, is important afterwards)
            if (existingRelationships.Count > 0)
            {
                Log.A($"found existing rels⋮{existingRelationships.Count}");
                // this is necessary after remove, because otherwise EF state tracking gets messed up
                DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.ToSicEavEntityRelationships.RemoveRange(existingRelationships));
            }

            // now save the changed relationships
            DbContext.DoAndSaveWithoutChangeDetection(() =>
                packages.ForEach(p =>
                {
                    var newEntityIds = p.Targets.ToList();
                    // Create new relationships which didn't exist before
                    for (var i = 0; i < newEntityIds.Count; i++)
                        DbContext.SqlDb.ToSicEavEntityRelationships.Add(new ToSicEavEntityRelationships
                        {
                            AttributeId = p.AttributeId,
                            ChildEntityId = newEntityIds[i],
                            SortOrder = i,
                            ParentEntityId = p.Entity.EntityId
                        });
                })); 

            wrapLog.Done("ok");
        }

        /// <summary>
        /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
        /// </summary>
        private void AddToQueue(int attributeId, List<Guid?> newValue, int entityId, bool flushAll)
        {
            Log.A($"add to queue for i:{entityId}, guids⋮{newValue.Count}");
            _saveQueue.Add(new RelationshipToSave
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
            _saveQueue.Add(new RelationshipToSave
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
            var wrapLog = Log.Fn("", startTimer: true);
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

                    Log.A($"will add relationships⋮{_saveQueue.Count}");

                    var parentIds = _saveQueue.Select(rel => rel.ParentEntityId).ToArray();
                    if (parentIds.Any(p => p <= 0)) throw new Exception("some parent has no id provided, can't update relationships");
                    var parents = DbContext.Entities.GetDbEntities(parentIds);
                    Log.A("Found parents to map:" + parents.Length);

                    var allTargets = _saveQueue
                        .Where(rel => rel.ChildEntityGuids != null)
                        .SelectMany(rel => rel.ChildEntityGuids.Where(g => g.HasValue).Select(g => g.Value))
                        .Distinct()
                        .ToArray();
                    Log.A("Total target IDs: " + allTargets.Length);
                    var dbTargetIds = DbContext.Entities.GetMostCurrentDbEntities(allTargets);
                    Log.A("Total target entities (should match): " + dbTargetIds.Count);

                    var updates = new List<RelationshipUpdatePackage>();
                    foreach (var relationship in _saveQueue)
                    {
                        var entity = parents.Single(e => e.EntityId == relationship.ParentEntityId);
                        // old
                        //var entity = relationship.ParentEntityId > 0
                        //    ? DbContext.Entities.GetDbEntity(relationship.ParentEntityId)
                        //    : throw new Exception("no id provided, can't update relationships");

                        // start with the ID list - or if it doesn't exist, a new list
                        var childEntityIds = relationship.ChildEntityIds ?? new List<int?>();

                        // if additional / alternative guids were specified, use those
                        if (childEntityIds.Count == 0 && relationship.ChildEntityGuids != null)
                        {
                            var dbIds = dbTargetIds;
                                //DbContext.Entities.GetMostCurrentDbEntities(relationship.ChildEntityGuids
                                //    .Where(g => g.HasValue).Select(g => g.Value).ToArray());
                            foreach (var childGuid in relationship.ChildEntityGuids)
                                try
                                {
                                    childEntityIds.Add(childGuid.HasValue
                                        ? dbIds.ContainsKey(childGuid.Value) ? dbIds[childGuid.Value] : new int?()
                                        : new int?());
                                }
                                catch (InvalidOperationException)
                                {
                                    // ignore, may occur if the child entity doesn't exist / wasn't created successfully
                                }

                            // old;
                            //foreach (var childGuid in relationship.ChildEntityGuids)
                            //    try
                            //    {
                            //        childEntityIds.Add(childGuid.HasValue
                            //            ? DbContext.Entities.GetMostCurrentDbEntity(childGuid.Value).EntityId
                            //            : new int?());
                            //    }
                            //    catch (InvalidOperationException)
                            //    {
                            //        // ignore, may occur if the child entity doesn't exist / wasn't created successfully
                            //    }

                        }
                        updates.Add(new RelationshipUpdatePackage(entity, relationship.AttributeId, childEntityIds));
                        //UpdateEntityRelationshipsAndSave(new List<RelationshipUpdatePackage>{ new RelationshipUpdatePackage(entity, relationship.AttributeId, childEntityIds)});
                    }
                    UpdateEntityRelationshipsAndSave(updates);
                }
            );

            wrapLog.Done();
            _saveQueue.Clear();
        }

        private class RelationshipUpdatePackage
        {
            public int AttributeId;
            public List<int?> Targets;
            public ToSicEavEntities Entity;

            public RelationshipUpdatePackage(ToSicEavEntities entity, int attributeId, List<int?> relationships)
            {
                Entity = entity;
                AttributeId = attributeId;
                Targets = relationships;
            }
        }

        internal void FlushChildrenRelationships(List<int> parentIds)
        {
            var wrapLog = Log.Fn($"{parentIds?.Count} items", message: "will do full-flush", startTimer: true);
            
            // Delete all existing relationships - but not the target, just the relationship
            // note: can't use .Clear(), as that will try to actually delete the children
            if (parentIds == null || parentIds.Count <= 0)
            {
                wrapLog.Done();
                return;
            }

            foreach (var id in parentIds)
            {
                var ent = DbContext.Entities.GetDbEntity(id);
                foreach (var relationToDelete in ent.RelationshipsWithThisAsParent)
                    DbContext.SqlDb.ToSicEavEntityRelationships.Remove(relationToDelete);
            }
            // intermediate save (important) so that EF state tracking works
            DbContext.SqlDb.SaveChanges();
            wrapLog.Done();
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

        internal void ChangeRelationships(IEntity eToSave, ToSicEavEntities dbEntity, List<ToSicEavAttributes> attributeDefs, SaveOptions so)
        {
            var wrapLog = Log.Fn(startTimer: true);

            // some initial error checking
            if(dbEntity.EntityId <= 0)
                throw new Exception("can't work on relationships if entity doesn't have a repository id yet");

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
                    var list = attribute.Values?.FirstOrDefault()?.ObjectContents;
                    switch (list)
                    {
                        case null:
                            continue;
                        case IEnumerable<IEntity> entities:
                            list = ((LazyEntities)entities).Identifiers;
                            break;
                        case Guid guid:
                            list = new List<Guid> {guid};
                            break;
                        case int i:
                            list = new List<int> {i};
                            break;
                    }

                    if (list is List<Guid> || list is List<Guid?>)
                    {
                        var guidList = (list as List<Guid>)?.Select(p => (Guid?) p) ??
                                       ((List<Guid?>) list).Select(p => p);
                        AddToQueue(attribDef.AttributeId, guidList.ToList(), dbEntity.EntityId, !so.PreserveUntouchedAttributes);
                    }
                    else if (list is List<int> || list is List<int?>)
                    {
                        var entityIds = list as List<int?> ?? ((List<int>) list).Select(v => (int?) v).ToList();
                        AddToQueue(attribDef.AttributeId, entityIds, dbEntity.EntityId, !so.PreserveUntouchedAttributes);
                    }
                }
            });
            wrapLog.Done();
        }
    }


}
