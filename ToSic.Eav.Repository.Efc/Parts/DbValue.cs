using System.Linq;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbValue : BllCommandBase
    {
        public DbValue(DbDataController cntx, Log parentLog = null) : base(cntx, parentLog, "DbVals")
        {
        }

        /// <summary>
        /// Copy all Values (including Related Entities) from teh Source Entity to the target entity
        /// </summary>
        internal void CloneEntitySimpleValues(ToSicEavEntities source, ToSicEavEntities target)
        {
            // Clear values on target (including Dimensions). Must be done in separate steps, would cause unallowed null-Foreign-Keys
            if (target.ToSicEavValues.Any(v => v.ChangeLogDeleted == null))
                foreach (var eavValue in target.ToSicEavValues.Where(v => v.ChangeLogDeleted == null))
                    eavValue.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();

            // Add all Values with Dimensions
            foreach (var eavValue in source.ToSicEavValues.ToList())
            {
                var value = new ToSicEavValues()
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
            }

            //#region copy relationships
            //CloneRelationshipsAndSave(source, target);
            //#endregion
        }

        internal void CloneRelationshipsAndSave(ToSicEavEntities source, ToSicEavEntities target)
        {
            DbContext.DoInTransaction(() =>
            {

                // note the related Entities are managed in the EntityParentRelationships. not sure why though
                // Delete all existing relationships - but not the target, just the relationship
                // note: can't use .Clear(), as that will try to actually delete the children
                //target.RelationshipsWithThisAsParent.Clear();
                foreach (var relationToDelete in target.RelationshipsWithThisAsParent)
                    DbContext.SqlDb.ToSicEavEntityRelationships.Remove(relationToDelete);
                // intermediate save (important) so that EF state tracking works
                DbContext.SqlDb.SaveChanges();

                // Add all Related Entities
                foreach (var entityParentRelationship in source.RelationshipsWithThisAsParent)
                    target.RelationshipsWithThisAsParent.Add(new ToSicEavEntityRelationships
                    {
                        AttributeId = entityParentRelationship.AttributeId,
                        ChildEntityId = entityParentRelationship.ChildEntityId,
                        SortOrder = entityParentRelationship.SortOrder
                    });
                DbContext.SqlDb.SaveChanges();
            });
        }
    }
}
