using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbValue : BllCommandBase
    {
        public DbValue(DbDataController db) : base(db, "Db.Values") {}

        /// <summary>
        /// Copy all Values (including Related Entities) from teh Source Entity to the target entity
        /// </summary>
        internal void CloneEntitySimpleValues(ToSicEavEntities source, ToSicEavEntities target)
        {
            Log.A($"CloneEntitySimpleValues({source.EntityId}, {target.EntityId})");
            // Clear values on target (including Dimensions). Must be done in separate steps, would cause un-allowed null-Foreign-Keys
            var delCount = 0;
            if (target.ToSicEavValues.Any(v => v.ChangeLogDeleted == null))
                foreach (var eavValue in target.ToSicEavValues.Where(v => v.ChangeLogDeleted == null))
                {
                    eavValue.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
                    delCount++;
                }

            // Add all Values with Dimensions
            var cloneCount = 0;
            foreach (var eavValue in source.ToSicEavValues.ToList())
            {
                var value = new ToSicEavValues
                {
                    AttributeId = eavValue.AttributeId,
                    Value = eavValue.Value,
                    ChangeLogCreated = DbContext.Versioning.GetChangeLogId()
                };

                // copy Dimensions
                foreach (var valuesDimension in eavValue.ToSicEavValuesDimensions)
                    value.ToSicEavValuesDimensions.Add(new ToSicEavValuesDimensions
                    {
                        DimensionId = valuesDimension.DimensionId,
                        ReadOnly = valuesDimension.ReadOnly
                    });

                target.ToSicEavValues.Add(value);
                cloneCount++;
            }
            Log.A($"DelCount: {delCount}, cloneCount:{cloneCount} (note: should be 0 if json)");
            Log.A($"/CloneEntitySimpleValues({source.EntityId}, {target.EntityId})");
        }

        internal void CloneRelationshipsAndSave(ToSicEavEntities source, ToSicEavEntities target)
        {
            Log.A($"CloneRelationshipsAndSave({source.EntityId}, {target.EntityId})");
            DbContext.DoInTransaction(() =>
            {
                // note the related Entities are managed in the EntityParentRelationships. not sure why though
                // Delete all existing relationships - but not the target, just the relationship
                // note: can't use .Clear(), as that will try to actually delete the children
                Log.A($"Flush relationships on {target.EntityId}");
                foreach (var relationToDelete in target.RelationshipsWithThisAsParent)
                    DbContext.SqlDb.ToSicEavEntityRelationships.Remove(relationToDelete);
                // intermediate save (important) so that EF state tracking works
                DbContext.SqlDb.SaveChanges();

                // Add all Related Entities
                Log.A($"add {source.RelationshipsWithThisAsParent.Count} relationships to {target.EntityId}");
                foreach (var entityParentRelationship in source.RelationshipsWithThisAsParent)
                    target.RelationshipsWithThisAsParent.Add(new ToSicEavEntityRelationships
                    {
                        ParentEntityId = target.EntityId,
                        AttributeId = entityParentRelationship.AttributeId,
                        ChildEntityId = entityParentRelationship.ChildEntityId,
                        SortOrder = entityParentRelationship.SortOrder
                    });
                DbContext.SqlDb.SaveChanges();
            });
        }
    }
}
