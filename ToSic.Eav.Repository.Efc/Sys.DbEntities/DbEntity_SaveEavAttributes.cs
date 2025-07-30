using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    /// <summary>
    /// Determine if queue is active
    /// </summary>
    private bool _attributeQueueActive;

    /// <summary>
    /// List of actions in the queue for saving
    /// </summary>
    //private readonly List<Action> _attributeUpdateQueue = [];
    private readonly List<TsDynDataValue> _valueUpdateQueue = [];

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
        var lMain = LogDetails.Fn($"id:{newEnt.EntityId}", timer: true);
        if (!_attributeQueueActive)
            throw new("Attribute save-queue not ready - should be wrapped");

        foreach (var value in newEnt.Attributes.Values)
        {
            var l = lMain.Fn($"InnerAttribute:{value.Name}");
            
            // find attribute definition
            var attribDef = dbAttributes
                .SingleOrDefault(a => string.Equals(a.StaticName, value.Name, StringComparison.InvariantCultureIgnoreCase));

            // If attribute definition missing, either throw or ignore
            if (attribDef == null)
            {
                if (!so.DiscardAttributesNotInType)
                    throw new($"trying to save attribute {value.Name} but can\'t find definition in DB");
                l.Done("attribute not found, will skip according to save-options");
                continue;
            }

            // Skip relationship attributes
            if (attribDef.Type == nameof(ValueTypes.Entity))
            {
                l.Done("type is entity, skip for now as relationships are processed later");
                continue;
            }

            foreach (var valueAtom in value.Values)
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
                _valueUpdateQueue.Add(newVal);
                //_attributeUpdateQueue.Add(() => DbContext.SqlDb.TsDynDataValues.Add(newVal));
            }

            l.Done();
        }
        lMain.Done();
    }

    internal void DoWhileQueueingAttributes(Action action)
    {
        var randomId = Guid.NewGuid().ToString().Substring(0, 4);
        var l = LogDetails.Fn($"attribute queue:{randomId} start");

        //if (_attributeUpdateQueue.Any())
        if (_valueUpdateQueue.Any())
            throw new("Attribute queue started while already containing stuff - bad!");

        _attributeQueueActive = true;

        // 3. now run the inner code
        action.Invoke();

        // 4. now check if we were the outermost call, in if yes, save the data
        DbStore.DoAndSaveWithoutChangeDetection(AttributeQueueAdd);
        _attributeQueueActive = false;
        l.Done();
    }

    private void AttributeQueueAdd()
    {
        var l = LogSummary.Fn($"Attributes: {_valueUpdateQueue.Count}", timer: true);
        //foreach (var a in _attributeUpdateQueue)
        //    a.Invoke();
        DbStore.SqlDb.TsDynDataValues.AddRange(_valueUpdateQueue);
        _valueUpdateQueue.Clear();
        //_attributeUpdateQueue.Clear();
        l.Done();
    }
}