using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Entities.Sys.Sources;

namespace ToSic.Eav.DataSources;

partial class TreeMapper
{
    private const string PrefixForNeeds = "Needs:";
    public IImmutableList<IEntity> AddParentChild(
        IEnumerable<IEntity> originals,
        string parentIdField,
        string childToParentRefField,
        string newChildrenField = default,
        string newParentField = default,
        LazyLookup<object, IEntity> lookup = default)
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        // Make sure we have field names in case they were not provided & full-clone entities/relationships
        newParentField = newParentField ?? DefaultParentFieldName;
        newChildrenField = newChildrenField ?? DefaultChildrenFieldName;
        lookup = lookup ?? new LazyLookup<object, IEntity>();

        // Prepare - figure out the parent IDs and Reference to Parent ID
        var withKeys = originals
            .Select(e =>
            {
                var ownId = GetKey(e, parentIdField);
                var relatedId = GetKey(e, childToParentRefField);
                return new EntityPair<(object OwnId, object RelatedId)>(e, (ownId, relatedId));
            })
            .ToList();

        var attrBld = _builder.Attribute;

        var result = withKeys.Select(pair =>
            {
                // Create list of the new attributes with the parent and child relationships
                var newAttributes = new List<IAttribute>
                {
                    attrBld.Relationship(newParentField, new List<object> { pair.Partner.RelatedId }, lookup),
                    attrBld.Relationship(newChildrenField, new List<object> { $"{PrefixForNeeds}{pair.Partner.OwnId}" }, lookup)
                };
                // Create combine list of attributes and generate an entity from that
                var attributes = attrBld.Replace(pair.Entity.Attributes, newAttributes);
                var newEntity = _builder.Entity.CreateFrom(pair.Entity, attributes: attrBld.Create(attributes));

                // Assemble a new pair, for later populating the lookup list
                // It's important to use the _new_ entity here, because the original is missing the new attributes
                return new EntityPair<(object OwnId, object RelatedId)>(newEntity, pair.Partner);
            })
            .ToList();

        // Add lookup to own id
        lookup.Add(result.Select(pair => new KeyValuePair<object, IEntity>(pair.Partner.OwnId, pair.Entity)));
        // Add list of "Needs:ParentId" so that the parents can find it
        lookup.Add(result.Select(pair => new KeyValuePair<object, IEntity>($"{PrefixForNeeds}{pair.Partner.RelatedId}", pair.Entity)));

        return l.Return(result.Select(r => r.Entity).ToImmutableList());
    }

    /// <summary>
    /// Gets the key for the specified field.
    /// It converts it to a `string` because that ensures that we'll always find it no matter how it was converted / extended with prefixes.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    private object GetKey(IEntity e, string attribute)
    {
        var l = Log.Fn<object>(enabled: Debug);
        try
        {
            var val = e.Get(attribute);
            l.A(Debug, $"Entity: {e.EntityId}[{attribute}]={val} ({val.GetType().Name})");
            return l.Return(val.ToString());
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            return l.ReturnAsError(default);
        }
    }
}