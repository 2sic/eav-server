using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    /// <summary>
    /// Remove values and attached dimensions of these values from the DB
    /// Important when updating json-entities, to ensure we don't keep trash around
    /// </summary>
    /// <param name="entityId"></param>
    private bool ClearValuesInDbUntracked(int entityId)
    {
        var l = LogDetails.Fn<bool>(timer: true);
        var val = DbStore.SqlDb.TsDynDataValues
            .Include(v => v.TsDynDataValueDimensions)
            .Where(v => v.EntityId == entityId)
            .ToList();

        if (val.Count == 0)
            return l.ReturnFalse("no changes");

        var dims = val.SelectMany(v => v.TsDynDataValueDimensions);
        DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.RemoveRange(dims));
        DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.RemoveRange(val));
        return l.ReturnTrue("ok");
    }

    internal void SaveAttributesAsEavUntracked(IEntity newEnt,
        SaveOptions so,
        List<TsDynDataAttribute> dbAttributes,
        int entityId,
        List<DimensionDefinition> zoneLangs,
        bool logDetails)
    {
        var listOfChanges = GetSaveAttributesAsEavUntracked(newEnt, so, dbAttributes, entityId, zoneLangs, logDetails);
        SaveAttributeChanges(listOfChanges);
    }

    internal ICollection<TsDynDataValue> GetSaveAttributesAsEavUntracked(IEntity newEnt,
        SaveOptions so,
        List<TsDynDataAttribute> dbAttributes,
        int entityId,
        List<DimensionDefinition> zoneLangs,
        bool logDetails)
    {
        var lMain = LogDetails.Fn<ICollection<TsDynDataValue>>($"id:{newEnt.EntityId}", timer: true);

        var listOfChanges = newEnt.Attributes.Values
            .SelectMany(value =>
            {
                var l = lMain.Fn<TsDynDataValue>($"InnerAttribute:{value.Name}");

                // find attribute definition
                var attribDef = dbAttributes
                    .SingleOrDefault(a =>
                        string.Equals(a.StaticName, value.Name, StringComparison.InvariantCultureIgnoreCase));

                // If attribute definition missing, either throw or ignore
                if (attribDef == null)
                {
                    if (!so.DiscardAttributesNotInType)
                        throw new($"trying to save attribute {value.Name} but can\'t find definition in DB");
                    l.Done("attribute not found, will skip according to save-options");
                    return [];
                }

                // Skip relationship attributes
                if (attribDef.Type == nameof(ValueTypes.Entity))
                {
                    l.Done("type is entity, skip for now as relationships are processed later");
                    return [];
                }

                var atomList = value.Values
                    .Select(valueAtom =>
                    {
                        #region prepare languages - has extensive error reporting, to help in case any db-data is bad

                        List<TsDynDataValueDimension>? toSicEavValuesDimensions;
                        try
                        {
                            toSicEavValuesDimensions = valueAtom.Languages
                                ?.Select(lng => new TsDynDataValueDimension
                                {
                                    DimensionId = zoneLangs
                                        .Single(ol => ol.Matches(lng.Key))
                                        .DimensionId,
                                    ReadOnly = lng.ReadOnly
                                })
                                .ToList();
                        }
                        catch (Exception ex)
                        {
                            throw new(
                                "something went wrong building the languages to save - " +
                                "your DB probably has some wrong language information which doesn't match; " +
                                "maybe even a duplicate entry for a language code" +
                                " - see https://github.com/2sic/2sxc/issues/1293",
                                ex);
                        }

                        #endregion

                        if (logDetails)
                            LogDetails.A(LogDetails.Try(() =>
                                $"add attrib:{attribDef.AttributeId}/{attribDef.StaticName} vals⋮{value.Values?.Count()}, dim⋮{toSicEavValuesDimensions?.Count}"));

                        var newVal = new TsDynDataValue
                        {
                            AttributeId = attribDef.AttributeId,
                            Value = valueAtom.Serialized ?? "",
                            TsDynDataValueDimensions = toSicEavValuesDimensions,
                            EntityId = entityId
                        };
                        return l.Return(newVal);
                    })
                    .ToList();
                return atomList;
            })
            .ToListOpt();

        return lMain.Return(listOfChanges);
    }

    internal void SaveAttributeChanges(ICollection<TsDynDataValue> changes)
    {
        var l = LogSummary.Fn($"Attributes: {changes.Count}", timer: true);
        DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.TsDynDataValues.AddRange(changes));
        l.Done();
    }
}