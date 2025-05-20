namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbEntity
{

    /// <summary>
    /// Remove values and attached dimensions of these values from the DB
    /// Important when updating json-entities, to ensure we don't keep trash around
    /// </summary>
    /// <param name="entityId"></param>
    private bool ClearAttributesInDbModel(int entityId)
    {
        var l = Log.Fn<bool>(timer: true);
        var val = DbContext.SqlDb.TsDynDataValues
            .Include(v => v.TsDynDataValueDimensions)
            .Where(v => v.EntityId == entityId)
            .ToList();

        if (val.Count == 0)
            return l.ReturnFalse("no changes");

        var dims = val.SelectMany(v => v.TsDynDataValueDimensions);
        DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.RemoveRange(dims));
        DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.RemoveRange(val));
        return l.ReturnTrue("ok");
    }

    private void SaveAttributesAsEav(IEntity newEnt,
        SaveOptions so,
        List<TsDynDataAttribute> dbAttributes,
        TsDynDataEntity dbEnt,
        int transactionId,
        bool logDetails)
    {
        var l = Log.Fn($"id:{newEnt.EntityId}", timer: true);
        if (!_attributeQueueActive)
            throw new("Attribute save-queue not ready - should be wrapped");
        foreach (var attribute in newEnt.Attributes.Values)
            Log.Do($"InnerAttribute:{attribute.Name}", () =>
            {
                // find attribute definition
                var attribDef =
                    dbAttributes.SingleOrDefault(
                        a => string.Equals(a.StaticName, attribute.Name,
                            StringComparison.InvariantCultureIgnoreCase));
                if (attribDef == null)
                {
                    if (!so.DiscardAttributesNotInType)
                        throw new(
                            $"trying to save attribute {attribute.Name} but can\'t find definition in DB");
                    return "attribute not found, will skip according to save-options";
                }

                if (attribDef.Type == ValueTypes.Entity.ToString())
                    return "type is entity, skip for now as relationships are processed later";

                foreach (var value in attribute.Values)
                {
                    #region prepare languages - has extensive error reporting, to help in case any db-data is bad

                    List<TsDynDataValueDimension> toSicEavValuesDimensions;
                    try
                    {
                        toSicEavValuesDimensions = value.Languages?.Select(l => new TsDynDataValueDimension
                        {
                            DimensionId = _zoneLangs.Single(ol => ol.Matches(l.Key)).DimensionId,
                            ReadOnly = l.ReadOnly
                        }).ToList();
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
                        Log.A(Log.Try(() =>
                            $"add attrib:{attribDef.AttributeId}/{attribDef.StaticName} vals⋮{attribute.Values?.Count()}, dim⋮{toSicEavValuesDimensions?.Count}"));

                    var newVal = new TsDynDataValue
                    {
                        AttributeId = attribDef.AttributeId,
                        Value = value.Serialized ?? "",
                        TsDynDataValueDimensions = toSicEavValuesDimensions,
                        EntityId = dbEnt.EntityId
                    };
                    AttributeQueueAdd(() => DbContext.SqlDb.TsDynDataValues.Add(newVal));
                }

                return "ok";
            });
        l.Done();
    }

    internal void DoWhileQueueingAttributes(Action action)
    {
        var randomId = Guid.NewGuid().ToString().Substring(0, 4);
        var l = Log.Fn($"attribute queue:{randomId} start");
        if (_attributeUpdateQueue.Any())
            throw new("Attribute queue started while already containing stuff - bad!");
        _attributeQueueActive = true;
        // 1. check if it's the outermost call, in which case afterwards we import
        //var willPurgeQueue = _isOutermostCall;
        // 2. make sure any follow-up calls are not regarded as outermost
        //_isOutermostCall = false;
        // 3. now run the inner code
        action.Invoke();
        // 4. now check if we were the outermost call, in if yes, save the data
        DbContext.DoAndSaveWithoutChangeDetection(AttributeQueueRun);
        _attributeQueueActive = false;
        l.Done();
    }

    // ReSharper disable once RedundantDefaultMemberInitializer
    private bool _attributeQueueActive = false;
    private readonly List<Action> _attributeUpdateQueue = [];

    private void AttributeQueueAdd(Action next) => _attributeUpdateQueue.Add(next);

    private void AttributeQueueRun()
    {
        var l = Log.Fn(timer: true);
        _attributeUpdateQueue.ForEach(a => a.Invoke());
        _attributeUpdateQueue.Clear();
        l.Done();
    }
}