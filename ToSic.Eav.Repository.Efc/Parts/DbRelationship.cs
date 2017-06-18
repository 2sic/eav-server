using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
        internal void AddToQueue(int attributeId, List<Guid?> newValue, /*Guid? entityGuid,*/ int? entityId)
        {
            _relationshipToSave.Add(new RelationshipToSave
            {
                AttributeId = attributeId,
                ChildEntityGuids = newValue,
                //ParentEntityGuid = entityGuid,
                ParentEntityId = entityId
            });
        }

        /// <summary>
        /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
        /// </summary>
        internal void AddToQueue(int attributeId, List<int?> newValue, /*Guid? entityGuid,*/ int? entityId)
        {
            _relationshipToSave.Add(new RelationshipToSave
            {
                AttributeId = attributeId,
                ChildEntityIds = newValue,
                //ParentEntityGuid = entityGuid,
                ParentEntityId = entityId
            });
        }

        /// <summary>
        /// Import Entity Relationships Queue (Populated by UpdateEntityRelationships) and Clear Queue afterward.
        /// </summary>
        internal void ImportRelationshipQueueAndSave()
        {
            foreach (var relationship in _relationshipToSave)
            {
                var entity = // relationship.ParentEntityGuid.HasValue
                    //? DbContext.Entities.GetMostCurrentDbEntity(relationship.ParentEntityGuid.Value)
                    // : 
                relationship.ParentEntityId.HasValue
                        ? DbContext.Entities.GetDbEntity(relationship.ParentEntityId.Value)
                        : null;
                if(entity == null)
                    throw new Exception("neither guid nor id provided, can't update relationships");

                // start with the ID list - or if it doesn't exist, a new list
                var childEntityIds = relationship.ChildEntityIds ?? new List<int?>();

                // if additional / alternative guids were specified, use those
                foreach (var childGuid in relationship.ChildEntityGuids)
                {
                    try
                    {
                        childEntityIds.Add(childGuid.HasValue
                            ? DbContext.Entities.GetMostCurrentDbEntity(childGuid.Value).EntityId
                            : new int?());
                    }
                    catch (InvalidOperationException)
                    {
                        // ignore, may occur if the child entity doesn't exist / wasn't created successfully
                    }
                }

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

            public int? ParentEntityId { get; set; }
        }

        #endregion
    }


}
