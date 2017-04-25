using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbRelationship: BllCommandBase
    {
        

        private readonly List<RelationshipToSave> _relationshipToSave = new List<RelationshipToSave>();


        public DbRelationship(DbDataController cntx) : base(cntx) {}

        /// <summary>
        /// Update Relationships of an Entity
        /// </summary>
        internal void UpdateEntityRelationships(int attributeId, IEnumerable<int?> newValue, ToSicEavEntities currentEntity)
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
        internal void AddToQueue(int attributeId, List<Guid?> newValue, Guid? entityGuid, int? entityId)
        {
            _relationshipToSave.Add(new RelationshipToSave
            {
                AttributeId = attributeId,
                ChildEntityGuids = newValue,
                ParentEntityGuid = entityGuid,
                ParentEntityId = entityId
            });
        }

        /// <summary>
        /// Import Entity Relationships Queue (Populated by UpdateEntityRelationships) and Clear Queue afterward.
        /// </summary>
        internal void ImportRelationshipQueue()
        {
            foreach (var relationship in _relationshipToSave)
            {
                var entity = relationship.ParentEntityGuid.HasValue 
                        ? DbContext.Entities.GetMostCurrentDbEntity(relationship.ParentEntityGuid.Value)
                        : DbContext.Entities.GetDbEntity(relationship.ParentEntityId.Value);
                var childEntityIds = new List<int?>();
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
                        // ignore, may occur if the child entity wasn't created successfully
                    }
                }

                UpdateEntityRelationships(relationship.AttributeId, childEntityIds, entity);
            }

            _relationshipToSave.Clear();
        }




        #region Internal Helper Classes

        private class RelationshipToSave
        {
            public int AttributeId { get; set; }
            public Guid? ParentEntityGuid { get; set; }
            public List<Guid?> ChildEntityGuids { get; set; }

            public int? ParentEntityId { get; set; }
        }

        #endregion
    }


}
