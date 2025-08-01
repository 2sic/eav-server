namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbValue(DbStorage.DbStorage db) : DbPartBase(db, "Db.Values")
{
    /// <summary>
    /// Copy all Values (including Related Entities) from teh Source Entity to the target entity
    /// </summary>
    internal ICollection<TsDynDataValue> CloneEntitySimpleValuesAndQueueUntracked(TsDynDataEntity source, TsDynDataEntity target)
    {
        var l = LogDetails.Fn<ICollection<TsDynDataValue>>($"CloneEntitySimpleValues({source.EntityId}, {target.EntityId})");
        
        // Clear values on target (including Dimensions). Must be done in separate steps, would cause un-allowed null-Foreign-Keys
        DbStore.SqlDb.RemoveRange(target.TsDynDataValues);

        // Add all Values with Dimensions
        var additions = source.TsDynDataValues
            .Select(eavValue =>
            {
                var value = new TsDynDataValue
                {
                    AttributeId = eavValue.AttributeId,
                    Value = eavValue.Value,
                    EntityId = target.EntityId,
                };

                // copy Dimensions; assign directly, since the value doesn't have an ID yet.
                foreach (var valuesDimension in eavValue.TsDynDataValueDimensions)
                    value.TsDynDataValueDimensions.Add(new()
                    {
                        DimensionId = valuesDimension.DimensionId,
                        ReadOnly = valuesDimension.ReadOnly
                    });

                return value;
            })
            .ToListOpt();

        DbStore.SqlDb.AddRange(additions);

        l.A($"DelCount: {target.TsDynDataValues.Count}, cloneCount:{additions.Count} (note: should be 0 if json)");
        return l.Return(additions, $"/CloneEntitySimpleValues({source.EntityId}, {target.EntityId})");
    }

    internal void CloneRelationshipsAndQueueUntracked(TsDynDataEntity source, TsDynDataEntity target)
    {
        var l = LogDetails.Fn($"CloneRelationshipsAndSave({source.EntityId}, {target.EntityId})");

        // note the related Entities are managed in the EntityParentRelationships. not sure why though
        // Delete all existing relationships - but not the target, just the relationship
        // note: can't use .Clear(), as that will try to actually delete the children
        l.A($"Flush relationships on {target.EntityId}; drop on target: {target.RelationshipsWithThisAsParent.Count}; from source: {source.RelationshipsWithThisAsParent.Count}");

        // Relationships to add; copied from the source entity
        var additions = source.RelationshipsWithThisAsParent
            .Select(entityParentRelationship => new TsDynDataRelationship
            {
                ParentEntityId = target.EntityId,
                AttributeId = entityParentRelationship.AttributeId,
                ChildEntityId = entityParentRelationship.ChildEntityId,
                SortOrder = entityParentRelationship.SortOrder
            })
            .ToListOpt();

        DbStore.SqlDb.RemoveRange(target.RelationshipsWithThisAsParent);
        DbStore.SqlDb.AddRange(additions);

        l.Done();
    }

    // Before 2025-07-29 remove ca. Q3 2025


    //internal void CloneEntitySimpleValues(TsDynDataEntity source, TsDynDataEntity target)
    //{
    //    var l = LogDetails.Fn($"CloneEntitySimpleValues({source.EntityId}, {target.EntityId})");
    //    // Clear values on target (including Dimensions). Must be done in separate steps, would cause un-allowed null-Foreign-Keys
    //    var delCount = 0;
    //    if (target.TsDynDataValues.Any())
    //        delCount += target.TsDynDataValues.Count();

    //    // Add all Values with Dimensions
    //    var cloneCount = 0;
    //    foreach (var eavValue in source.TsDynDataValues.ToList())
    //    {
    //        var value = new TsDynDataValue
    //        {
    //            AttributeId = eavValue.AttributeId,
    //            Value = eavValue.Value
    //        };

    //        // copy Dimensions
    //        foreach (var valuesDimension in eavValue.TsDynDataValueDimensions)
    //            value.TsDynDataValueDimensions.Add(new()
    //            {
    //                DimensionId = valuesDimension.DimensionId,
    //                ReadOnly = valuesDimension.ReadOnly
    //            });

    //        target.TsDynDataValues.Add(value);
    //        cloneCount++;
    //    }
    //    l.A($"DelCount: {delCount}, cloneCount:{cloneCount} (note: should be 0 if json)");
    //    l.A($"/CloneEntitySimpleValues({source.EntityId}, {target.EntityId})");
    //    l.Done();
    //}

    //internal void CloneRelationshipsAndSave(TsDynDataEntity source, TsDynDataEntity target)
    //{
    //    var l = LogDetails.Fn($"CloneRelationshipsAndSave({source.EntityId}, {target.EntityId})");
    //    DbContext.DoInTransaction(() =>
    //    {
    //        // note the related Entities are managed in the EntityParentRelationships. not sure why though
    //        // Delete all existing relationships - but not the target, just the relationship
    //        // note: can't use .Clear(), as that will try to actually delete the children
    //        l.A($"Flush relationships on {target.EntityId}");
    //        foreach (var relationToDelete in target.RelationshipsWithThisAsParent)
    //            DbContext.SqlDb.TsDynDataRelationships.Remove(relationToDelete);
    //        // intermediate save (important) so that EF state tracking works
    //        DbContext.SqlDb.SaveChanges();

    //        // Add all Related Entities
    //        l.A($"add {source.RelationshipsWithThisAsParent.Count} relationships to {target.EntityId}");
    //        foreach (var entityParentRelationship in source.RelationshipsWithThisAsParent)
    //            target.RelationshipsWithThisAsParent.Add(new()
    //            {
    //                ParentEntityId = target.EntityId,
    //                AttributeId = entityParentRelationship.AttributeId,
    //                ChildEntityId = entityParentRelationship.ChildEntityId,
    //                SortOrder = entityParentRelationship.SortOrder
    //            });
    //        DbContext.SqlDb.SaveChanges();
    //    });
    //    l.Done();
    //}

}