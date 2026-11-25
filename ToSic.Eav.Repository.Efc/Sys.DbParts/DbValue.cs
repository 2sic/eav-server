namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbValue(DbStorage.DbStorage dbStore) : HelperBase(dbStore.Log, "Db.Values")
{
    private ILog? LogDetails => dbStore.LogDetails == null ? null : field ??= Log;

    /// <summary>
    /// Copy all Values (including Related Entities) from teh Source Entity to the target entity
    /// </summary>
    internal ICollection<TsDynDataValue> CloneEntitySimpleValuesAndQueueUntracked(TsDynDataEntity source, TsDynDataEntity target)
    {
        var l = LogDetails.Fn<ICollection<TsDynDataValue>>($"CloneEntitySimpleValues({source.EntityId}, {target.EntityId})");
        
        // Clear values on target (including Dimensions). Must be done in separate steps, would cause un-allowed null-Foreign-Keys
        dbStore.SqlDb.RemoveRange(target.TsDynDataValues);

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

        dbStore.SqlDb.AddRange(additions);

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

        dbStore.SqlDb.RemoveRange(target.RelationshipsWithThisAsParent);
        dbStore.SqlDb.AddRange(additions);

        l.Done();
    }

}