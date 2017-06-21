﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbRelationship: BllCommandBase
    {
        

        private readonly List<RelationshipToSave> _relationshipToSave = new List<RelationshipToSave>();


        public DbRelationship(DbDataController cntx) : base(cntx) {}

        internal ICollection<ToSicEavEntityRelationships> GetRelationshipsOfParent(int parentId)
        {
            return DbContext.SqlDb.ToSicEavEntityRelationships
                .Include(r => r.ChildEntity)
                .Include(r => r.Attribute)
                .Where(r => r.ParentEntityId == parentId)
                .ToList();
        }

        /// <summary>
        /// Update Relationships of an Entity
        /// </summary>
        private void UpdateEntityRelationshipsAndSave(int attributeId, IEnumerable<int?> newValue, ToSicEavEntities currentEntity)
        {
            // remove existing Relationships that are not in new list
            var newEntityIds = newValue.ToList();
            var existingRelationships = currentEntity.RelationshipsWithThisAsParent
                .Where(e => e.AttributeId == attributeId).ToList();

            // Delete all existing relationships (important, because the order, which is part of the key, is important afterwards)
            foreach (var relationToDelete in existingRelationships)
                DbContext.SqlDb.ToSicEavEntityRelationships.Remove(relationToDelete);
            DbContext.SqlDb.SaveChanges();  // this is necessary after remove, because otherwise EF state tracking gets messed up

            // Create new relationships which didn't exist before
            for (var i = 0; i < newEntityIds.Count; i++)
            {
                var newEntityId = newEntityIds[i];
                currentEntity.RelationshipsWithThisAsParent.Add(new ToSicEavEntityRelationships
                {
                    AttributeId = attributeId,
                    ChildEntityId = newEntityId,
                    SortOrder = i
                });
            }

            DbContext.SqlDb.SaveChanges(); // now save the changed relationships
        }

        /// <summary>
        /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
        /// </summary>
        internal void AddToQueue(int attributeId, List<Guid?> newValue, int entityId, bool flushAll)
        {
            _relationshipToSave.Add(new RelationshipToSave
            {
                AttributeId = attributeId, ChildEntityGuids = newValue, ParentEntityId = entityId
            });
        }

        /// <summary>
        /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
        /// </summary>
        internal void AddToQueue(int attributeId, List<int?> newValue, int entityId, bool flushAll)
        {
            _relationshipToSave.Add(new RelationshipToSave
            {
                AttributeId = attributeId, ChildEntityIds = newValue, ParentEntityId = entityId, FlushAllEntityRelationships = flushAll
            });
        }

        /// <summary>
        /// Import Entity Relationships Queue (Populated by UpdateEntityRelationships) and Clear Queue afterward.
        /// </summary>
        internal void ImportRelationshipQueueAndSave()
        {
            // todo: if so determines it, clear all existing relationships first
            var fullFlush = _relationshipToSave
                .Where(r => r.FlushAllEntityRelationships)
                .Select(r => r.ParentEntityId)
                .GroupBy(id => id)
                .Select(g => g.First())
                .ToList();

            if (fullFlush.Count > 0)
            {
                foreach (var id in fullFlush)
                {
                    var ent = DbContext.Entities.GetDbEntity(id);

                    // Delete all existing relationships - but not the target, just the relationship
                    // note: can't use .Clear(), as that will try to actually delete the children
                    foreach (var relationToDelete in ent.RelationshipsWithThisAsParent)
                        DbContext.SqlDb.ToSicEavEntityRelationships.Remove(relationToDelete);

                }
                DbContext.SqlDb.SaveChanges(); // this is necessary after remove, because otherwise EF state tracking gets messed up
            }

            foreach (var relationship in _relationshipToSave)
            {
                var entity = relationship.ParentEntityId > 0
                    ? DbContext.Entities.GetDbEntity(relationship.ParentEntityId)
                    : null;

                if(entity == null)
                    throw new Exception("no id provided, can't update relationships");

                // start with the ID list - or if it doesn't exist, a new list
                var childEntityIds = relationship.ChildEntityIds ?? new List<int?>();

                // if additional / alternative guids were specified, use those
                if(childEntityIds.Count == 0 && relationship.ChildEntityGuids != null)
                    foreach (var childGuid in relationship.ChildEntityGuids)
                        try
                        {
                            childEntityIds.Add(childGuid.HasValue
                                ? DbContext.Entities.GetMostCurrentDbEntity(childGuid.Value).EntityId
                                : new int?());
                        }
                        catch (InvalidOperationException) { } // ignore, may occur if the child entity doesn't exist / wasn't created successfully

                UpdateEntityRelationshipsAndSave(relationship.AttributeId, childEntityIds, entity);
            }

            _relationshipToSave.Clear();
        }




        #region Internal Helper Classes

        private struct RelationshipToSave
        {
            public int AttributeId { get; set; }
            //public Guid? ParentEntityGuid { get; set; }
            public List<Guid?> ChildEntityGuids { get; set; }
            public List<int?> ChildEntityIds { get; set; }

            public int ParentEntityId { get; set; }


            public bool FlushAllEntityRelationships { get; set; }
        }

        #endregion

        internal void SaveRelationships(IEntity eToSave, ToSicEavEntities dbEntity, List<ToSicEavAttributes> attributeDefs, SaveOptions so)
        {
            // some initial error checking
            if(dbEntity.EntityId <= 0)
                throw new Exception("can't work on relationships if entity doesn't have a repository id yet");

            // todo: put all relationships into queue
            foreach (var attribute in eToSave.Attributes.Values)
            {                    
                // find attribute definition
                var attribDef = attributeDefs.Single(a => string.Equals(a.StaticName, attribute.Name, StringComparison.InvariantCultureIgnoreCase));
                if (attribDef.Type != AttributeTypeEnum.Entity.ToString()) continue;

                var list = attribute.Values?.FirstOrDefault()?.ObjectContents;
                if (list == null) continue;
                //var attribId = attribDef.AttributeId;

                if (list is Guid) list = new List<Guid> {(Guid) list};
                if (list is List<Guid> || list is List<Guid?>)
                {
                    var guidList = (list as List<Guid>)?.Select(p => (Guid?)p) ?? ((List<Guid?>)list).Select(p => p);
                    AddToQueue(attribDef.AttributeId, guidList.ToList(), dbEntity.EntityId, true);
                }

                if (list is int) list = new List<int> {(int) list};
                if (list is EntityRelationship) list = ((EntityRelationship) list).EntityIds.ToList();
                if (list is List<int> || list is List<int?>)
                {
                    var entityIds = list as List<int?> ?? ((List<int>)list).Select(v => (int?)v).ToList();
                    DbContext.Relationships.AddToQueue(attribDef.AttributeId, entityIds, dbEntity.EntityId, true);
                }

            }

            // probably parse queue if SO determines so, or let the outside layer determine this
            if (!so.DelayRelationshipSave)
                ImportRelationshipQueueAndSave();
        }
    }


}
