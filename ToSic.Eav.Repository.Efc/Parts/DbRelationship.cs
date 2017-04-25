using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
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
        internal void UpdateEntityRelationships(int attributeId, IEnumerable<int?> newValue, ToSicEavEntities currentEntity)
        {
            // remove existing Relationships that are not in new list
            var newEntityIds = newValue.ToList();
            var existingRelationships =
                currentEntity.RelationshipsWithThisAsParent/*.EntityParentRelationships*/.Where(e => e.AttributeId == attributeId).ToList();

            // Delete all existing relationships (important, because the order, which is part of the key, is important afterwards)
            foreach (var relationToDelete in existingRelationships)//.Where(r => !newEntityIds.Contains(r.ChildEntityId)))
                DbContext.SqlDb.ToSicEavEntityRelationships.Remove(relationToDelete);

            DbContext.SqlDb.SaveChanges();

            // Create new relationships which didn't exist before
            var reallyNewIds = newEntityIds;//.Where(n => !existingRelationships.Select(r => r.ChildEntityId).Contains(n)).ToList();
            for (var i = 0; i < /*newEntityIds*/reallyNewIds.Count; i++)
            {
                var newEntityId = /*newEntityIds*/reallyNewIds[i];
                currentEntity.RelationshipsWithThisAsParent/*.EntityParentRelationships*/.Add(new ToSicEavEntityRelationships
                {
                    AttributeId = attributeId,
                    ChildEntityId = newEntityId,
                    //ParentEntityId = currentEntity.EntityId,
                    SortOrder = i
                });
            }

            // new: now sort them all
            //for (var i = 0; i < newEntityIds.Count; i++)
            //{
            //    var newEntityId = newEntityIds[i];
            //    var rel = currentEntity.RelationshipsWithThisAsParent.Single(r => r.AttributeId == attributeId && r.ChildEntityId == newEntityId);
            //    rel.SortOrder = i;
            //}

            // test code
            DbContext.SqlDb.SaveChanges();
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
                        ? DbContext.Entities.GetMostCurrentDbEntity(relationship.ParentEntityGuid.Value)
                        : DbContext.Entities.GetDbEntity(relationship.ParentEntityId.Value);
                var childEntityIds = new List<int?>();
                foreach (var childGuid in relationship.ChildEntityGuids)
                {
                    try
                    {
                        childEntityIds.Add(childGuid.HasValue ? DbContext.Entities.GetMostCurrentDbEntity(childGuid.Value).EntityId : new int?());
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
