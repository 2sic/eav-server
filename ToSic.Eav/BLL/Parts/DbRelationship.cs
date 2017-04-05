using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.BLL.Parts
{
    public class DbRelationship: BllCommandBase
    {
        

        private readonly List<EntityRelationshipQueueItem> _entityRelationshipsQueue =
            new List<EntityRelationshipQueueItem>();


        public DbRelationship(DbDataController cntx) : base(cntx)
        {
        }

        /// <summary>
        /// Update Relationships of an Entity
        /// </summary>
        internal void UpdateEntityRelationships(int attributeId, IEnumerable<int?> newValue, Entity currentEntity)
        {
            // remove existing Relationships that are not in new list
            var newEntityIds = newValue.ToList();
            var existingRelationships =
                currentEntity.EntityParentRelationships.Where(e => e.AttributeID == attributeId).ToList();

            // Delete all existing relationships
            foreach (var relationToDelete in existingRelationships)
                DbContext.SqlDb.EntityRelationships.DeleteObject(relationToDelete);

            // Create new relationships
            for (var i = 0; i < newEntityIds.Count; i++)
            {
                var newEntityId = newEntityIds[i];
                currentEntity.EntityParentRelationships.Add(new EntityRelationship
                {
                    AttributeID = attributeId,
                    ChildEntityID = newEntityId,
                    SortOrder = i
                });
            }
        }

        /// <summary>
        /// Update Relationships of an Entity. Update isn't done until ImportEntityRelationshipsQueue() is called!
        /// </summary>
        internal void UpdateEntityRelationships(int attributeId, List<Guid?> newValue, Guid? entityGuid, int? entityId)
        {
            _entityRelationshipsQueue.Add(new EntityRelationshipQueueItem
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
        internal void ImportEntityRelationshipsQueue()
        {
            foreach (var relationship in _entityRelationshipsQueue)
            {
                var entity = relationship.ParentEntityGuid.HasValue 
                        ? DbContext.Entities.GetDbEntity(relationship.ParentEntityGuid.Value)
                        : DbContext.Entities.GetDbEntity(relationship.ParentEntityId.Value);
                var childEntityIds = new List<int?>();
                foreach (var childGuid in relationship.ChildEntityGuids)
                {
                    try
                    {
                        childEntityIds.Add(childGuid.HasValue ? DbContext.Entities.GetDbEntity(childGuid.Value).EntityID : new int?());
                    }
                    catch (InvalidOperationException)
                    {
                    } // may occur if the child entity wasn't created successfully
                }

                UpdateEntityRelationships(relationship.AttributeId, childEntityIds, entity);
            }

            _entityRelationshipsQueue.Clear();
        }




        #region Internal Helper Classes

        private class EntityRelationshipQueueItem
        {
            public int AttributeId { get; set; }
            public Guid? ParentEntityGuid { get; set; }
            public List<Guid?> ChildEntityGuids { get; set; }

            public int? ParentEntityId { get; set; }
        }

        #endregion
    }


}
